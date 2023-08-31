using Bogus.Bson;
using Humanizer;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Api
{
    public partial class CaseController
    {
        [HttpPost("bulkSave")]
        [AllowAnonymous]
        public async virtual Task<ItemResult> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory
        )
        {
            // We suppress any retry strategies because we mutate the input state as
            // we go along. If the operation fails, the user can just click the save button
            // in the UI again.
            return await new SqlServerRetryingExecutionStrategy(Db, 0).ExecuteAsync<ItemResult>(async () =>
            {
                using var tran = await Db.Database.BeginTransactionAsync();

                // Security:
                foreach (var entity in dto.Save)
                {
                    entity.Initialize(dataSourceFactory, behaviorsFactory);

                    var kind = await entity.DetermineSaveKindAsync(parameters);
                    var declaredFor = entity.DeclaredForClassViewModel;

                    if (kind == SaveKind.Create && !declaredFor.SecurityInfo.IsCreateAllowed(User))
                    {
                        return $"You are not permitted to create {declaredFor.DisplayName} items.";
                    }
                    if (kind == SaveKind.Update && !declaredFor.SecurityInfo.IsEditAllowed(User))
                    {
                        return $"You are not permitted to edit {declaredFor.DisplayName} items.";
                    }
                }

                foreach (var entity in dto.Delete)
                {
                    entity.Initialize(dataSourceFactory, behaviorsFactory);

                    var declaredFor = entity.DeclaredForClassViewModel;
                    if (!declaredFor.SecurityInfo.IsDeleteAllowed(User))
                    {
                        return $"You are not permitted to delete {declaredFor.DisplayName} items.";
                    }
                }



                // Naive implemenation - does not consider order of operations.
                foreach (var entity in dto.Delete)
                {
                    var result = await entity.DeleteAsync(parameters);
                    if (!result.WasSuccessful) return result;
                }

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

                        foreach (var reference in item.Refs ?? new(0))
                        {
                            if (reference.Key == item.PrimaryRefName)
                            {
                                // This is the reference identifier for this entity.
                                // It doesn't need to resolve to anything, so skip it.
                                continue;
                            }

                            var referencedProp = item.DtoClassViewModel.PropertyByName(reference.Key);
                            if (referencedProp is { IsClientWritable: false } or not { Role: PropertyRole.ForeignKey})
                            {
                                // Ignore invalid refs
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
                        if (!result.WasSuccessful) return result;

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

                await tran.CommitAsync();

                return true;
            });
        }
    }

    public class BulkSaveRequest
    {
        public List<BulkSaveRequestItem> Save { get; set; } = new();
        public List<BulkSaveRequestItem> Delete { get; set; } = new();

        private Dictionary<int, BulkSaveRequestItem>? _RefsLookup;
        internal Dictionary<int, BulkSaveRequestItem> RefsLookup
        {
            get
            {
                if (_RefsLookup is not null) return _RefsLookup;

                _RefsLookup = new();
                foreach (var item in Save)
                {
                    if (item.PrimaryRef is int val)
                    {
                        _RefsLookup[val] = item;
                    }
                }
                return _RefsLookup;
            }
        }
    }

    [JsonConverter(typeof(BulkSaveRequestItemConverter))]
    public abstract class BulkSaveRequestItem
    {
        public string Type { get; set; } = null!;

        /// <summary>
        /// The ClassViewModel for the entity type.
        /// </summary>
        internal ClassViewModel ClassViewModel { get; set; } = null!;

        /// <summary>
        /// The ClassViewModel for the DTO type.
        /// </summary>
        internal ClassViewModel DtoClassViewModel { get; set; } = null!;

        internal ClassViewModel DeclaredForClassViewModel => DtoClassViewModel.Type.IsA(typeof(GeneratedDto<>)) 
            ? ClassViewModel // Strategies and annotations for generated DTOs are always declared on the entity itself
            : DtoClassViewModel // Strategies and annotations for non-generated DTOs (i.e. custom DTOs) will be declared for that custom DTO.
            ;

        public Dictionary<string, int>? Refs { get; set; }

        /// <summary>
        /// An IClassDto containing the data for the item.
        /// </summary>
        public object Data { get; set; } = null!;

        internal PropertyViewModel DtoPrimaryKey => DtoClassViewModel.PrimaryKey ?? throw new Exception("Bulk saves cannot operate on unkeyed models");

        internal object? PrimaryKey => DtoPrimaryKey.PropertyInfo.GetValue(Data);

        internal string PrimaryRefName => DtoPrimaryKey.JsonName;
        internal int? PrimaryRef => Refs?.TryGetValue(PrimaryRefName, out int v) == true ? v : null;

        internal abstract void Initialize(IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory);

        internal abstract Task<SaveKind> DetermineSaveKindAsync(DataSourceParameters parameters);
        internal abstract Task<ItemResult> SaveAsync(DataSourceParameters parameters);
        internal abstract Task<ItemResult> DeleteAsync(DataSourceParameters parameters);
    }

    public class BulkSaveRequestItem<T, TDto> : BulkSaveRequestItem
        where T : class
        where TDto : class, IClassDto<T>, new()
    {
        /// <summary>
        /// An IClassDto containing the data for the item.
        /// </summary>
        public TDto Dto
        {
            get => (TDto)Data;
            set => Data = value;
        }

        internal IDataSource<T>? DataSource { get; set; }
        internal IBehaviors<T>? Behaviors { get; set; }

        internal override async Task<SaveKind> DetermineSaveKindAsync(DataSourceParameters parameters)
        {
            return (await Behaviors!.DetermineSaveKindAsync<TDto>(Dto, DataSource!, parameters)).Kind;
        }

        internal override async Task<ItemResult> SaveAsync(DataSourceParameters parameters)
        {
            var result = await Behaviors!.SaveAsync<TDto>(Dto, DataSource!, parameters);
            if (!result.WasSuccessful) return result;

            Dto = result.Object ?? Dto;

            return result;
        }

        internal override async Task<ItemResult> DeleteAsync(DataSourceParameters parameters)
        {
            return await Behaviors!.DeleteAsync<TDto>(PrimaryKey, DataSource!, parameters);
        }

        internal override void Initialize(IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory)
        {
            if (DataSource is not null) throw new InvalidOperationException("Already initialized!");

            ClassViewModel declaredFor = DeclaredForClassViewModel;
            DataSource = dataSourceFactory.GetDefaultDataSource<T>(declaredFor);
            Behaviors = behaviorsFactory.GetBehaviors<T>(declaredFor);
        }

        
    }

    public class BulkSaveRequestItemConverter : JsonConverter<BulkSaveRequestItem>
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0#support-polymorphic-deserialization

        public override BulkSaveRequestItem Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

            string? propertyName = reader.GetString();
            if (propertyName?.Equals("type", StringComparison.OrdinalIgnoreCase) != true)
            {
                throw new JsonException("The first property in the items in a bulk save must be 'Type'.");
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.String) throw new JsonException();

            string typeName = reader.GetString() ?? throw new JsonException();

            // TODO: fix O(n) lookup:
            // TODO: Use ReflectionRepository from DI if possible?
            var type = ReflectionRepository.Global.CrudApiBackedClasses
                .FirstOrDefault(cvm => cvm.ClientTypeName == typeName);

            if (type is null)
            {
                throw new JsonException($"Unknown type '{typeName}'");
            }

            var entityType = type.BaseViewModel;
            var dtoTypeName = type.DtoName;

            var dtoType = (type.IsDto
                ? type.Type.TypeInfo
                : Type.GetType("Coalesce.Web.Vue3.Models." + dtoTypeName)) // todo: fix this
                ?? throw new JsonException($"Cannot construct '{dtoTypeName}'");

            BulkSaveRequestItem ret = (BulkSaveRequestItem)Activator.CreateInstance(
                typeof(BulkSaveRequestItem<,>).MakeGenericType(
                    entityType.Type.TypeInfo,
                    dtoType
                ))!;

            ret.Type = typeName;
            ret.ClassViewModel = entityType;
            ret.DtoClassViewModel = ReflectionRepository.Global.GetClassViewModel(dtoType)!;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return ret;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName?.ToLowerInvariant())
                    {
                        case "data":
                            ret.Data = JsonSerializer.Deserialize(ref reader, dtoType, options)
                                ?? throw new JsonException("Property 'data' is missing");
                            break;
                        case "refs":
                            ret.Refs = JsonSerializer.Deserialize<Dictionary<string, int>>(ref reader, options);
                            break;
                    }
                }
            }

            return ret;
        }

        public override void Write(
            Utf8JsonWriter writer,
            BulkSaveRequestItem dto,
            JsonSerializerOptions options) 
            => throw new NotSupportedException();
    }
}
