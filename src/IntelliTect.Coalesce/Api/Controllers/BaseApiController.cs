using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api.Controllers
{
    public abstract class BaseApiController<T, TDto> : Controller
        where T : class
        where TDto : class, IClassDto<T>, new()
    {
        protected BaseApiController(CrudContext context)
        {
            Context = context;
            EntityClassViewModel = context.ReflectionRepository.GetClassViewModel<T>()!;
        }

        /// <summary>
        /// Contains contextual information about the request.
        /// </summary>
        protected CrudContext Context { get; }

        /// <summary>
        /// A ClassViewModel representing the entity type T that is served by this controller,
        /// independent of the DTO that will encapsulate the type in inputs and outputs.
        /// </summary>
        protected ClassViewModel EntityClassViewModel { get; }

        /// <summary>
        /// For generated controllers, the type that the controller was generated for.
        /// For custom IClassDtos, this is the DTO type. Otherwise, this is the entity type.
        /// </summary>
        protected ClassViewModel? GeneratedForClassViewModel { get; set; }

        protected Task<ItemResult<TDto>> GetImplementation(object id, DataSourceParameters parameters, IDataSource<T> dataSource)
        {
            return dataSource.GetMappedItemAsync<TDto>(id, parameters);
        }

        protected Task<ListResult<TDto>> ListImplementation(ListParameters listParameters, IDataSource<T> dataSource)
        {
            return dataSource.GetMappedListAsync<TDto>(listParameters);
        }

        protected Task<ItemResult<int>> CountImplementation(FilterParameters parameters, IDataSource<T> dataSource)
        {
            return dataSource.GetCountAsync(parameters);
        }

        protected async Task<ItemResult<TDto?>> SaveImplementation(TDto dto, DataSourceParameters parameters, IDataSource<T> dataSource, IBehaviors<T> behaviors)
        {
            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, parameters)).Kind;

            if (GeneratedForClassViewModel is null)
            {
                throw new InvalidOperationException($"{nameof(GeneratedForClassViewModel)} must be set.");
            }

            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
            {
                Response.StatusCode = User.Identity?.IsAuthenticated == true 
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
                return $"Creation of {GeneratedForClassViewModel.DisplayName} items not allowed.";
            }

            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
            {
                Response.StatusCode = User.Identity?.IsAuthenticated == true
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
                return $"Editing of {GeneratedForClassViewModel.DisplayName} items not allowed.";
            }

            return await behaviors.SaveAsync(dto, dataSource, parameters);
        }

        protected Task<ItemResult<TDto?>> DeleteImplementation(object id, DataSourceParameters parameters, IDataSource<T> dataSource, IBehaviors<T> behaviors)
        {
            return behaviors.DeleteAsync<TDto>(id, dataSource, parameters);
        }
    }

    public abstract class BaseApiController<T, TDto, TContext> : BaseApiController<T, TDto>
        where T : class
        where TDto : class, IClassDto<T>, new()
        where TContext : DbContext
    {
        protected BaseApiController(CrudContext<TContext> context) : base(context)
        {
        }

        /// <summary>
        /// Contains contextual information about the request.
        /// </summary>
        public new CrudContext<TContext> Context => (CrudContext<TContext>)base.Context;

        public TContext Db => Context.DbContext;

        protected async Task<ItemResult<TDto>> BulkSaveImplementation(
            BulkSaveRequest dto,
            DataSourceParameters parameters,
            IDataSourceFactory dataSourceFactory,
            IBehaviorsFactory behaviorsFactory
        )
        {
            if (dto is null && HttpContext.RequestServices.GetService(typeof(IJsonHelper))?.GetType().Name.Contains("Newtonsoft") == true)
            {
                throw new NotSupportedException("Coalesce bulk saves require that the server be configured to use System.Text.Json, not Newtonsoft.Json.");
            }

            if (dto is null)
            {
                throw new ArgumentException("Request body could not be read. There may have been a deserialization error.");
            }

            var strategy = Db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<ItemResult<TDto>>(async () =>
            {
                using var tran = await Db.Database.BeginTransactionAsync();

                try
                {
                    BulkSaveRequestItem? root = null;

                    // Security:
                    foreach (var item in dto.Save)
                    {
                        item.Initialize(dataSourceFactory, behaviorsFactory);

                        var kind = await item.DetermineSaveKindAsync(parameters);
                        var declaredFor = item.DeclaredForClassViewModel;

                        if (kind == SaveKind.Create && !declaredFor.SecurityInfo.IsCreateAllowed(User))
                        {
                            return $"You are not permitted to create {declaredFor.DisplayName} items.";
                        }
                        if (kind == SaveKind.Update && !declaredFor.SecurityInfo.IsEditAllowed(User))
                        {
                            return $"You are not permitted to edit {declaredFor.DisplayName} items.";
                        }

                        if (root is null &&
                            item.Root && 
                            item.Type.Equals(GeneratedForClassViewModel?.ClientTypeName, StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            root = item;
                        }
                    }

                    foreach (var item in dto.Delete)
                    {
                        item.Initialize(dataSourceFactory, behaviorsFactory);

                        var declaredFor = item.DeclaredForClassViewModel;
                        if (!declaredFor.SecurityInfo.IsDeleteAllowed(User))
                        {
                            return $"You are not permitted to delete {declaredFor.DisplayName} items.";
                        }
                    }

                    // If we didn't find a root, it might have an action of `none`
                    // (i.e. it exists in the items soley for the purpose of declaring itself as the root).
                    root ??= dto.None.FirstOrDefault(i => i.Root);


                    var itemsToSave = new List<BulkSaveRequestItem>(dto.Save);
                    // Keep looping over the items to save until we've saved them all,
                    // or until we get stuck. We do this because reference between items
                    // will progressively get resolved as more items are saved.
                    while (itemsToSave.Count > 0)
                    {
                        var iterationStartCount = itemsToSave.Count;
                        for (int i = 0; i < itemsToSave.Count; i++)
                        {
                            var item = itemsToSave[i];

                            if (item.Refs is not null) foreach (var reference in item.Refs)
                            {
                                if (reference.Key == item.PrimaryRefName)
                                {
                                    // This is the reference identifier for this entity.
                                    // It doesn't need to resolve to anything, so skip it.
                                    continue;
                                }

                                var referencedProp = item.DtoClassViewModel.PropertyByName(reference.Key);
                                if (referencedProp is not { Role: PropertyRole.ForeignKey, IsClientWritable: true })
                                {
                                    // Ignore invalid refs. We only need to resolve writable foreign keys.
                                    continue;
                                }

                                // This is a FK reference to another entity in the operation.
                                // See if we can resolve it.
                                var principalItem = dto.RefsLookup.GetValueOrDefault(reference.Value);
                                var principalKey = principalItem?.PrimaryKey;
                                if (principalKey is null)
                                {
                                    // Can't resolve this entity yet.
                                    goto nextItem;
                                }

                                // Update the DTO with the new FK.
                                referencedProp.PropertyInfo.SetValue(item.Data, principalKey);
                            }

                            // If we made it here, there were no refs preventing a save.
                            // Save the entity. This will update `item.Dto`/`item.Data`
                            // with the result of the save, which will in turn make the new PK available.
                            var result = await item.SaveAsync(parameters);

                            // TODO: better feedback about which specific entity failed?
                            if (!result.WasSuccessful) return new(result);

                            // Remove it from the list.
                            itemsToSave.RemoveAt(i);
                            i--;

                            nextItem:;
                        }

                        if (iterationStartCount == itemsToSave.Count)
                        {
                            // In this iteration over all remaining saves,
                            // we weren't able to complete any.
                            // This means there are unresolvable references.

                            // Throw here, don't return an item result, because
                            // this is either a problem with Coalesce itself,
                            // or a problem with something the app developer did.
                            // It is not a user-actionable problem.
                            throw new InvalidOperationException("Unable to resolve one or more references for bulk save in the following entities:\n " +
                                string.Join('\n', itemsToSave.Select(i =>
                                    $"{i.Type} {i.PrimaryKey ?? $"(refs.{i.PrimaryRefName}: {i.PrimaryRef})"}")));


                            // TODO: Handle circular relationships with nullable FKs.
                        }
                    }


                    // Naive implemenation - does not consider order of operations.
                    // We do deletes second under the assumption that if a principal 
                    // entity is being deleted, the saves above may be removing the references
                    // to that principal entity so that there are no conflicts when deleting it.
                    // This is naive and a proper implementation will analyze relationships
                    // and interweave saves and deletes as needed. However, this is really hard.
                    foreach (var item in dto.Delete)
                    {
                        var result = await item.DeleteAsync(parameters);
                        if (!result.WasSuccessful) return new(result);
                    }

                    var refMap = dto.Save
                        .Where(i => i.PrimaryRef.HasValue)    
                        .ToDictionary(i => i.PrimaryRef!.Value, i => i.PrimaryKey);
                    if (root?.PrimaryKey is null)
                    {
                        await tran.CommitAsync();
                        return new(true)
                        {
                            RefMap = refMap
                        };
                    }
                    else
                    {
                        // Read security is implemented by the generated controller action.
                        var rootDataSource = dataSourceFactory.GetDataSource<T>(GeneratedForClassViewModel!, parameters.DataSource);
                        var rootResult = await GetImplementation(root.PrimaryKey, parameters, rootDataSource);

                        await tran.CommitAsync();

                        rootResult.RefMap = refMap;
                        return rootResult;
                    }
                }
                catch (Exception)
                {
                    tran.Rollback();
                    Db.ChangeTracker.Clear();
                    throw;
                }
            });
        }
    }
}
