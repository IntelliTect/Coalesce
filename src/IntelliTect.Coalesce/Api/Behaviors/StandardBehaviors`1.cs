using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce;

public abstract class StandardBehaviors<T> : IBehaviors<T>, IStandardCrudStrategy
    where T : class
{
    /// <summary>
    /// Contains contextual information about the request.
    /// </summary>
    public CrudContext Context { get; }

    /// <summary>
    /// The user making the request.
    /// </summary>
    public ClaimsPrincipal User => Context.User;

    /// <summary>
    /// A ClassViewModel representing the type T that is handled by the behaviors.
    /// </summary>
    public ClassViewModel ClassViewModel { get; set; }


    /// <summary>
    /// If set, this data source will be used in place of the supplied data source
    /// when retrieving an object that will be updated in a Save operation.
    /// </summary>
    public IDataSource<T>? OverrideFetchForUpdateDataSource { get; protected set; }

    /// <summary>
    /// If set, this data source will be used in place of the supplied data source
    /// when retrieving an object that will be deleted.
    /// </summary>
    public IDataSource<T>? OverrideFetchForDeleteDataSource { get; protected set; }

    /// <summary>
    /// If set, this data source will be used in place of the supplied data source
    /// when reloading an object after a save operation has completed.
    /// This is not recommended, as it can cause a client to receive unexpected results.
    /// </summary>
    public IDataSource<T>? OverridePostSaveResultDataSource { get; protected set; }

    /// <summary>
    /// If set, this data source will be used in place of the supplied data source
    /// when reloading an object after a delete operation has completed.
    /// This is not recommended, as it can cause a client to receive unexpected results.
    /// </summary>
    public IDataSource<T>? OverridePostDeleteResultDataSource { get; protected set; }

    /// <summary>
    /// If true, Coalesce will perform validation of incoming data using <see cref="ValidationAttribute"/>s
    /// present on your models during save operations (in <see cref="ValidateDto(SaveKind, IParameterDto{T})"/>).
    /// This setting defaults to the value of <see cref="CoalesceOptions.ValidateAttributesForSaves"/>.
    /// </summary>
    public bool ValidateAttributesForSaves { get; set; }

    public StandardBehaviors(CrudContext context)
    {
        Context = context
            ?? throw new ArgumentNullException(nameof(context));

        ClassViewModel = Context.ReflectionRepository.GetClassViewModel<T>()
            ?? throw new ArgumentException("Generic type T has no ClassViewModel.", nameof(T));

        ValidateAttributesForSaves = context.Options?.ValidateAttributesForSaves ?? ValidateAttributesForSaves;
    }


    #region Save

    /// <summary>
    /// From the incoming DTO, determines if the operation should be a create or update operation.
    /// Also discerns the primary key of the object that is being operated upon.
    /// </summary>
    /// <typeparam name="TDto">The type of the incoming DTO.</typeparam>
    /// <param name="incomingDto">The DTO to obtain a primary key from.</param>
    /// <param name="dataSource">The data source that will be used to check if the item exists if the item does not use a database-generated primary key.</param>
    /// <param name="parameters">The parameters to be passed to the data source when loading the item.</param>
    /// <returns>A SaveKind indicating either Create or Update, 
    /// and the value of the primary key that can be used for database lookups.</returns>
    public virtual async Task<(SaveKind Kind, object? IncomingKey)> DetermineSaveKindAsync<TDto>(
        TDto incomingDto,
        IDataSource<T> dataSource,
        IDataSourceParameters parameters
    )
        where TDto : class, IParameterDto<T>, new()
    {
        var dtoClassViewModel = Context.ReflectionRepository.GetClassViewModel<TDto>()!;
        var pk = ClassViewModel.PrimaryKey ?? throw new InvalidOperationException("Data sources cannot save items that lack a PK");
        PropertyViewModel dtoPkInfo;

        if (incomingDto is IGeneratedParameterDto<T> genDto)
        {
            // For generated DTOs, simplify by matching the PK name since it will always match.
            dtoPkInfo = dtoClassViewModel.PropertyByName(pk.Name)!;
        }
        else
        {
            // This path is really only for custom DTOs
            dtoPkInfo = dtoClassViewModel.PrimaryKey ?? throw new InvalidOperationException("Data sources cannot save items that lack a PK");
        }

        object? idValue = dtoPkInfo.PropertyInfo.GetValue(incomingDto);

        if (pk.DatabaseGenerated == DatabaseGeneratedOption.None)
        {
            // PK is not database generated.
            // We have to look for the existing object to determine if
            // this is a create or update.

            if (idValue == null)
            {
                // Pretend this is a create, even though it will definitely fail.
                // We just don't really want to handle the error in this method,
                // largely because it does not return an ItemResult.
                // So, we'll pass it down the line as if it was a create and let the
                // database and/or other parts of the behaviors (including any custom validation)
                // handle this as an error.
                return (SaveKind.Create, null);
            }

            var existingItem = await (OverrideFetchForUpdateDataSource ?? dataSource).GetItemAsync(idValue, parameters);
            if (existingItem.WasSuccessful && existingItem.Object != null)
            {
                return (SaveKind.Update, idValue);
            }

            return (SaveKind.Create, null);
        }

        // IsNullable handles nullable value types, and reference types (mainly strings).
        // !IsNullable handles non-Nullable<T> value types.
        if (dtoPkInfo.Type.IsReferenceOrNullableValue
            ? idValue == null
            : idValue!.Equals(Activator.CreateInstance(dtoPkInfo.Type.TypeInfo)))
        {
            return (SaveKind.Create, null);
        }
        else
        {
            return (SaveKind.Update, idValue);
        }
    }

    /// <summary>
    /// Fetches a fresh copy of the object from the data source after a save has been performed.
    /// </summary>
    /// <param name="dataSource">The data source that will be used when loading the item.</param>
    /// <param name="parameters">The parameters to be passed to the data source when loading the item.</param>
    /// <param name="item">The saved item to reload</param>
    protected virtual async Task<ItemResult<T>> FetchObjectAfterSaveAsync(IDataSource<T> dataSource, IDataSourceParameters parameters, T item)
    {
        var newItemId = ClassViewModel.PrimaryKey!.PropertyInfo.GetValue(item)!;
        var ds = (OverridePostSaveResultDataSource ?? dataSource);
        var result = await ds.GetItemAsync(newItemId, parameters);

        if (result.Object != null && ds is IResultTransformer<T> transformer)
        {
            await transformer.TransformResultsAsync(Array.AsReadOnly(new[] { result.Object }), parameters);
        }

        return result;
    }

    /// <summary>
    /// Maps the incoming DTO's properties to the item that will be saved to the database.
    /// </summary>
    /// <param name="kind">Discriminator between a create and a update operation.</param>
    /// <param name="item">The item that will be saved to the database.</param>
    /// <param name="dto">The incoming item from the client.</param>
    /// <param name="parameters">The additional parameters sent by the client.</param>
    protected virtual T MapIncomingDto<TDto>(SaveKind kind, T? item, TDto dto, IDataSourceParameters parameters)
        where TDto : class, IParameterDto<T>, new()
    {
        var context = new MappingContext(Context, parameters.Includes);
        if (kind == SaveKind.Create)
        {
            return dto.MapToNew(context);
        }
        else
        {
            dto.MapToModel(item!, context);
            return item!;
        }
    }

    /// <summary>
    /// Code to run before mapping the DTO to its model type.
    /// Allows for the chance to perform validation on the DTO itself rather than the mapped model in <see cref="BeforeSave(SaveKind, T, T)"/>.
    /// For generated DTOs where the type is not available, there are a variety of methods for retrieving expected 
    /// properties from the object based on its model type, although reflection is always an option as well.
    /// For behaviors on custom DTOs, a simple cast will allow access to all properties.
    /// </summary>
    /// <param name="kind">Discriminator between a create and a update operation.</param>
    /// <param name="dto">The incoming item from the client.</param>
    /// <returns></returns>
    public virtual ItemResult ValidateDto(SaveKind kind, IParameterDto<T> dto)
    {
        if (!ValidateAttributesForSaves) return true;
        return ItemResult.FromValidation(dto, deep: false, forceRequired: kind == SaveKind.Create, Context.ServiceProvider);
    }

    /// <summary>
    /// Code to run before committing a save to the database.
    /// Any changes made to the properties of <c>item</c> will be persisted to the database.
    /// The return a failure result will halt the save operation and return any associated message to the client.
    /// </summary>
    /// <param name="kind">Discriminator between a create and a update operation.</param>
    /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
    /// If kind == SaveKind.Create, this will be null.</param>
    /// <param name="item">An entity instance with its properties set to incoming, new values.</param>
    /// <returns>An ItemResult potentially indicating failure, upon which the save operation will halt without persisting changes.</returns>
    public virtual ItemResult BeforeSave(SaveKind kind, T? oldItem, T item) => true;

    /// <inheritdoc cref=" BeforeSave(SaveKind, T?, T)"/>
    public virtual Task<ItemResult> BeforeSaveAsync(SaveKind kind, T? oldItem, T item) => Task.FromResult(BeforeSave(kind, oldItem, item));

    /// <summary>
    /// Save the specified item to the database.
    /// </summary>
    /// <typeparam name="TDtoIn">
    ///     The type of the DTO that contains the data to be saved.
    /// </typeparam>
    /// <typeparam name="TDtoOut">
    ///     The type of DTO that the caller expects to receive the results in.
    /// </typeparam>
    /// <param name="incomingDto">The DTO containing the properties to update.</param>
    /// <param name="dataSource">The data source that will be used when loading the item to be updated.</param>
    /// <param name="parameters">The parameters to be passed to the data source when loading the item.</param>
    /// <returns>A result indicating success or failure, as well as an up-to-date copy of the object being saved.</returns>
    public virtual async Task<ItemResult<TDtoOut?>> SaveAsync<TDtoIn, TDtoOut>(
        TDtoIn incomingDto,
        IDataSource<T> dataSource,
        IDataSourceParameters parameters
    )
        where TDtoIn : class, IParameterDto<T>, new()
        where TDtoOut : class, IResponseDto<T>, new()
    {
        (SaveKind kind, object? idValue) = await DetermineSaveKindAsync(incomingDto, dataSource, parameters);

        T? originalItem = null;
        T? item = null;
        IncludeTree? includeTree;

        if (kind == SaveKind.Update)
        {
            // Primary Key was defined. This object should exist in the database.
            var existingItem = await (OverrideFetchForUpdateDataSource ?? dataSource).GetItemAsync(idValue!, parameters);
            if (!existingItem.WasSuccessful)
            {
                return new ItemResult<TDtoOut?>(existingItem);
            }
            item = existingItem.Object ?? throw new InvalidOperationException(
                $"Expected {nameof(ItemResult)}{nameof(ItemResult<T>.Object)} to be non-null when {nameof(dataSource.GetItemAsync)} returns success.");

            // Create a shallow copy.
            originalItem = item.Copy();
        }

        // Allow validation on the raw DTO before its been mapped.
        var validateDto = ValidateDto(kind, incomingDto);
        if (validateDto == null)
        {
            throw new InvalidOperationException("Received null from result of ValidateDto. Expected an ItemResult.");
        }

        if (!validateDto.WasSuccessful)
        {
            return new ItemResult<TDtoOut?>(validateDto);
        }

        // Set all properties on the DB-mapped object to the incoming values.
        item = MapIncomingDto(kind, item, incomingDto, parameters) ?? item!;

        // Allow interception of the save.
        var beforeSave = await BeforeSaveAsync(kind, originalItem, item);
        if (beforeSave == null)
        {
            throw new InvalidOperationException("Received null from result of BeforeSave. Expected an ItemResult.");
        }
        else if (!beforeSave.WasSuccessful)
        {
            return new ItemResult<TDtoOut?>(beforeSave);
        }

        try
        {
            await ExecuteSaveAsync(kind, originalItem, item);
        }
        catch (Exception ex)
        {
            var exResult = GetExceptionResult(ex, incomingDto);
            if (exResult is not null) return new ItemResult<TDtoOut?>(exResult);
            throw;
        }

        // Pull the object to get any changes.
        ItemResult<T> newItem = await FetchObjectAfterSaveAsync(dataSource, parameters, item);
        includeTree = newItem.IncludeTree;

        if (!newItem.WasSuccessful)
        {
            return $"The item was saved, but could not be loaded with the requested data source: {newItem.Message}";
        }

        item = newItem.Object ?? throw new InvalidOperationException(
            $"Expected {nameof(ItemResult)}{nameof(ItemResult<T>.Object)} to be non-null when {nameof(FetchObjectAfterSaveAsync)} returns success.");

        // Call the AfterSave method to allow the user to
        // modify the returned object, the include tree,
        // or signal an error.
        var afterSave = await AfterSaveAsync(kind, originalItem, item);
        if (afterSave == null)
        {
            throw new InvalidOperationException("Received null from result of AfterSaveAsync. Expected an ItemResult.");
        }
        else if (!afterSave.WasSuccessful)
        {
            return new ItemResult<TDtoOut?>(afterSave);
        }
        else
        {
            item = afterSave.Object ?? item;
            includeTree = afterSave.IncludeTree ?? includeTree;
        }

        return new ItemResult<TDtoOut?>(
            item.MapToDto<T, TDtoOut>(new MappingContext(Context, parameters.Includes), includeTree)
        );
    }

    /// <summary>
    /// Executes the save action against the database and persists the changes.
    /// </summary>
    /// <param name="kind">Discriminator between a create and a update operation.</param>
    /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
    /// If kind == SaveKind.Create, this will be null.</param>
    /// <param name="item">An entity instance with its properties set to incoming, new values.</param>
    public abstract Task ExecuteSaveAsync(SaveKind kind, T? oldItem, T item);

    /// <summary>
    /// Code to run after a save has been committed to the database. Allows any cleanup code to run, 
    /// as well as potential replacement of the object that will be returned to the client.
    /// </summary>
    /// <param name="kind">Discriminator between a create and a update operation.</param>
    /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
    /// If kind == SaveKind.Create, this will be null.</param>
    /// <param name="item">
    /// A fresh copy of the modified item retrieved from the database,
    /// complete with any relations that were included as a result of being loaded 
    /// from the dataSource that was specified by the client.
    /// </param>
    /// <returns>
    /// If a non-successful <see cref="ItemResult"/> is returned, a failure response will be 
    /// returned immediately without the updated item attached to the response.
    /// If a successful <see cref="ItemResult{T}"/> is returned, then a non-null <see cref="ItemResult{T}.Object"/> 
    /// on the result will override the item sent in the response, and a non-null <see cref="ApiResult.IncludeTree"/> 
    /// on the result will override the include tree used to map that item to the DTO. If these properties are left null 
    /// (e.g. you return <see langword="true"/>), <paramref name="item"/> will be returned in the response to the client.
    /// </returns>
    public virtual Task<ItemResult<T>> AfterSaveAsync(SaveKind kind, T? oldItem, T item) => Task.FromResult<ItemResult<T>>(true);

    #endregion

    #region Delete

    /// <summary>
    /// Delete the item with the specified key from the database.
    /// </summary>
    /// <param name="id">The primary key of the object to delete.</param>
    /// <param name="dataSource">The data source that will be used when loading the item to be deleted.</param>
    /// <param name="parameters">The parameters to be passed to the data source when loading the item.</param>
    /// <returns>A result indicating success or failure, 
    /// potentially including an up-to-date copy of the item being deleted if the delete action is non-destructive.</returns>
    public virtual async Task<ItemResult<TDto?>> DeleteAsync<TDto>(
        object id,
        IDataSource<T> dataSource,
        IDataSourceParameters parameters)
        where TDto : class, IResponseDto<T>, new()
    {
        var existingItem = await (OverrideFetchForDeleteDataSource ?? dataSource).GetItemAsync(id, parameters);
        if (!existingItem.WasSuccessful)
        {
            return existingItem.Message;
        }

        var item = existingItem.Object ?? throw new InvalidOperationException(
            $"Expected {nameof(ItemResult)}{nameof(ItemResult<T>.Object)} to be non-null when {nameof(dataSource.GetItemAsync)} returns success.");

        var beforeDelete = await BeforeDeleteAsync(item);
        if (beforeDelete == null)
        {
            throw new InvalidOperationException("Received null from result of BeforeDelete. Expected an ItemResult.");
        }

        if (!beforeDelete.WasSuccessful)
        {
            return new ItemResult<TDto?>(beforeDelete);
        }

        // Perform the delete operation against the database.
        // By default, this removes the item from its DbSet<> and calls SaveChanges().
        // This might be overridden to set a deleted flag on the object instead.
        try
        {
            await ExecuteDeleteAsync(item);
        }
        catch (Exception ex)
        {
            var exResult = GetExceptionResult(ex, null);
            if (exResult is not null) return new ItemResult<TDto?>(exResult);
            throw;
        }


        // Pull the object to see if it can still be seen by the user.
        // If so, the operation was a soft delete and the user is allowed to see soft-deleted items.
        var postDeleteDs = OverridePostDeleteResultDataSource ?? dataSource;
        var postDeleteGetResult = await postDeleteDs.GetItemAsync(id, parameters);
        var includeTree = postDeleteGetResult.IncludeTree;
        var deletedItem = postDeleteGetResult.Object;

        var afterDelete = await AfterDeleteAsync(deletedItem ?? item);
        if (afterDelete == null)
        {
            throw new InvalidOperationException("Received null from result of AfterDeleteAsync. Expected an ItemResult.");
        }
        else if (!afterDelete.WasSuccessful)
        {
            return new ItemResult<TDto?>(afterDelete);
        }
        else
        {
            deletedItem = afterDelete.Object ?? deletedItem;
            includeTree = afterDelete.IncludeTree ?? includeTree;
        }

        if (deletedItem == null)
        {
            // If the item was not retrieved, it was actually deleted.
            // Don't return the item to the client since it no longer exists (in the context of the applicable data source, anyway).
            return true;
        }

        // If the item WAS retrieved, it was soft deleted and still visible to the current user through a data source.
        // We return it to the client to signal two things:
        // 1) Update the item on which delete was called with the new object.
        // 2) Don't remove the item from its parent collections on the client, since it still exists through the requested data source.

        if (postDeleteDs is IResultTransformer<T> transformer)
        {
            await transformer.TransformResultsAsync(Array.AsReadOnly(new[] { deletedItem }), parameters);
        }

        return new ItemResult<TDto?>(
            deletedItem.MapToDto<T, TDto>(new MappingContext(Context, parameters.Includes), includeTree)
        );
    }

    /// <summary>
    /// Code to run before committing the delete operation to the database.
    /// If a failure result is returned, the delete operation will not execute.
    /// This method is called by DeleteAsync.
    /// This may be used to implement row-level security.
    /// </summary>
    /// <param name="item">The item being deleted.</param>
    /// <returns>An ItemResult that, if indicating failure, will halt the delete operation.</returns>
    public virtual ItemResult BeforeDelete(T item) => true;

    /// <summary>
    /// Code to run before committing the delete operation to the database.
    /// If a failure result is returned, the delete operation will not execute.
    /// This method is called by DeleteAsync.
    /// This may be used to implement row-level security.
    /// </summary>
    /// <param name="item">The item being deleted.</param>
    /// <returns>An ItemResult that, if indicating failure, will halt the delete operation.</returns>
    public virtual Task<ItemResult> BeforeDeleteAsync(T item) => Task.FromResult(BeforeDelete(item));

    /// <summary>
    /// Executes the delete action against the database and saves the change.
    /// This may be overridden to change what action is actually performed against the database 
    /// on deletion of an item (e.g. setting a deleted flag instead of deleting the row).
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public abstract Task ExecuteDeleteAsync(T item);

    /// <summary>
    /// Code to run after the delete operation is committed to the database.
    /// This can be used to perform cleanup or any other desired actions.
    /// </summary>
    /// <param name="item">
    /// The item that was deleted.
    /// If the item still exists in the database, this will be a a fresh copy of the item retrieved from the database,
    /// complete with any relations that were included as a result of being loaded 
    /// from the dataSource that was specified by the client.
    /// </param>
    /// <returns>
    /// If a non-successful <see cref="ItemResult"/> is returned, a failure response will be 
    /// returned immediately without the updated item attached to the response.
    /// If a successful <see cref="ItemResult{T}"/> is returned, then a non-null <see cref="ItemResult{T}.Object"/> 
    /// on the result will override the item sent in the response, and a non-null <see cref="ApiResult.IncludeTree"/> 
    /// on the result will override the include tree used to map that item to the DTO. If these properties are left null 
    /// (e.g. you return <see langword="true"/>), <paramref name="item"/> will be returned in the response to the client
    /// if it still exists in the database, or `null` if the item doesn't still exist in the database.
    /// </returns>
    public virtual Task<ItemResult<T>> AfterDeleteAsync(T item) => Task.FromResult<ItemResult<T>>(true);

    #endregion

    /// <summary>
    /// Attempt to transform a database exception into a user-friendly error message.
    /// Requires <see cref="CoalesceOptions.DetailedEfConstraintExceptionMessages"/> to be enabled.
    /// </summary>
    /// <param name="ex">The database exception that was thrown by EF</param>
    /// <param name="incomingDto">The incoming dto that the current operation is consuming, if any. Used to distinguish errors that were triggered by the user's input, as opposed to errors triggered by custom code in the behaviors implementation.</param>
    public virtual ItemResult? GetExceptionResult(Exception ex, IParameterDto<T>? incomingDto)
    {
        if (!Context.Options.DetailedEfConstraintExceptionMessages) return null;

        if (ex is not DbUpdateException dbUpdateException)
        {
            return null;
        }

        IModel? model = dbUpdateException.Entries.FirstOrDefault()?.Metadata.Model;

        if (dbUpdateException.InnerException is not DbException dbException || model is null)
        {
            return null;
        }

        // The INSERT statement conflicted with the FOREIGN KEY constraint "FK_CaseProduct_Product_ProductId". The conflict occurred in database "CoalesceDb", table "dbo.Product", column 'ProductId'.
        // The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_CaseProduct_Product_ProductId". The conflict occurred in database "CoalesceDb", table "dbo.Product", column 'ProductId'.
        // The DELETE statement conflicted with the REFERENCE constraint "FK_CaseProduct_Product_ProductId". The conflict occurred in database "CoalesceDb", table "dbo.CaseProduct", column 'ProductId'.
        Match match = Regex.Match(
            dbException.Message,
            @"(?<kind>INSERT|UPDATE|DELETE) statement conflicted with the (?:FOREIGN KEY|REFERENCE) constraint ""(?<constraint>[^""]+)""\. The conflict occurred in database ""[^""]+"", table ""(?<table>[^""]+)"", column '(?<column>[^']+)'");

        if (match.Success)
        {
            string kind = match.Groups["kind"].Value;
            string constraint = match.Groups["constraint"].Value;
            string table = match.Groups["table"].Value;
            string column = match.Groups["column"].Value;

            var conflictedTable = model
                .GetEntityTypes()
                .Where(t =>
                    t.GetSchemaQualifiedTableName() == table ||
                    (t.GetSchema() is null && table.EndsWith('.' + t.GetTableName()))
                )
                .FirstOrDefault();

            if (conflictedTable is null) return null;

            if (kind is "DELETE")
            {
                if (
                    // This operation isn't deleting this single entity, so it might have been some other entity being deleted that triggered the violation.
                    dbUpdateException.Entries.Any(entry => entry.State == EntityState.Deleted && entry.Metadata.ClrType != typeof(T) && !entry.Metadata.IsOwned())
                )
                {
                    return null;
                }

                var dependent = Context.ReflectionRepository.GetClassViewModel(conflictedTable.ClrType);
                var referencedBy = dependent?.Type.IsInternalUse != false
                    ? "other item" // Hide the type's name if it's internal or unknown
                    : dependent.DisplayName;

                return $"The {this.ClassViewModel.DisplayName} is still referenced by at least one {referencedBy}.";
            }

            var fk = conflictedTable.GetReferencingForeignKeys().Where(f => f.GetConstraintName() == constraint).FirstOrDefault();
            var dependentEntity = fk?.DeclaringEntityType;
            var referenceNav = fk?.DependentToPrincipal;

            if (
                referenceNav is not null &&
                dependentEntity is not null &&
                dependentEntity.ClrType == typeof(T) &&
                Context.ReflectionRepository.GetClassViewModel(dependentEntity.ClrType) is ClassViewModel dependentCvm &&
                dependentCvm.PropertyByName(referenceNav.Name) is PropertyViewModel referenceNavPvm
            )
            {
                // Find the FK prop that was changed. This will filter out an internal part of an FK like TenantId.
                var changedFkProp = incomingDto is ISparseDto sparse
                    ? fk!.Properties.FirstOrDefault(p => sparse.ChangedProperties.Contains(p.Name))
                    : fk!.Properties.FirstOrDefault(p => dependentCvm.PropertyByName(p.Name)?.SecurityInfo.Read.IsAllowed(User) == true);

                // Check that the user was actually changing this prop
                // (rather than the backend manually setting it in the behaviors).
                // This will also enforce that the prop is at least writable under *some*
                // circumstances and isn't read-only or internal through Coalesce.
                if (changedFkProp is not null)
                {
                    var message = $"The value of {referenceNavPvm.DisplayName} is not valid.";
                    return new(false, message, [new ValidationIssue(changedFkProp.Name, message)]);
                }
            }
        }

        // Cannot insert duplicate key row in object 'dbo.Product' with unique index 'IX_Product_ProductUniqueId'. The duplicate key value is (acab7c64-5cbd-472f-8f06-e442c037eda9)
        // Cannot insert duplicate key row in object 'dbo.Table_1' with unique index 'IX_Unique_Foo_Bar'. The duplicate key value is (as,df, gh,jk).
        match = Regex.Match(
            dbException.Message,
            @"Cannot insert duplicate key row in object '(?<table>[^""]+)' with unique index '(?<constraint>[^""]+)'. The duplicate key value is \((?<keyValue>[^']+)\)");

        if (match.Success)
        {
            string constraint = match.Groups["constraint"].Value;
            string tableName = match.Groups["table"].Value;
            string keyValue = match.Groups["keyValue"].Value;

            var table = model
                .GetEntityTypes()
                .FirstOrDefault(t =>
                    t.GetSchemaQualifiedTableName() == tableName ||
                    (t.GetSchema() is null && tableName.EndsWith('.' + t.GetTableName()))
                );

            var index = table?.GetIndexes().Where(f => f.GetDatabaseName() == constraint).FirstOrDefault();

            if (
                index is not null &&
                table is not null &&
                table.ClrType == typeof(T) &&
                Context.ReflectionRepository.GetClassViewModel(table.ClrType) is ClassViewModel dependentCvm
            )
            {
                // The value may be contained in "keyValue" pulled from the database error,
                // but SQL Server doesn't quote strings in the error message, so we don't really
                // know how to find the right value since there could be commas in the middle of strings.
                // So, find the affected entity ourselves by reconstructing the error message:
                var entity = dbUpdateException.Entries
                    // Find the entity described by the error message
                    .FirstOrDefault(entry =>
                        entry.Metadata.Equals(table) &&
                        keyValue == string.Join(", ", index.Properties.Select(p => entry.CurrentValues[p] ?? "<NULL>"))
                    );

                if (entity is null)
                {
                    return null;
                }

                // Reconstruct the violated unique values using only the values that the user is allowed to read.
                // This will eliminate internal parts of the constraint like a TenantId.

                var mappingContext = new MappingContext(Context);
                var propViewModels = index.Properties
                    .Select(p => dependentCvm.PropertyByName(p.Name)!)
                    .ToList();

                // Check that the user was actually changing one of the props in the index
                // (rather than the backend manually setting it in the behaviors).
                // This will also enforce that the prop is at least writable under *some*
                // circumstances and isn't read-only or internal through Coalesce.
                if (incomingDto is ISparseDto sparse && !propViewModels.Any(p => sparse.ChangedProperties.Contains(p.Name)))
                {
                    return null;
                }

                var propsWithSecurity = propViewModels.ConvertAll(p => new
                {
                    Prop = p,
                    UserCanRead = p.SecurityInfo.IsReadAllowed(mappingContext, entity.Entity),
                });

                // Only make this a user-friendly error if the user is allowed to read all parts of the index,
                // or if the unreadable parts of the index are internal use (which allows a TenantId to be excluded
                // while still presenting the rest of the props to the user).
                if (!propsWithSecurity.All(p => p.UserCanRead || p.Prop.IsInternalUse)) return null;

                var valuesDisplay = propsWithSecurity
                    .Where(p => p.UserCanRead)
                    .Select(p =>
                    {
                        var value = entity.CurrentValues[p.Prop.Name];
                        if (value is null)
                        {
                            value = "<NULL>";
                        }
                        else if (!p.Prop.Type.IsNumber)
                        {
                            // Quote non-numbers so its clear what part of the message is the actual value
                            value = $"'{value}'";
                        }

                        return $"{p.Prop.DisplayName} {value}";
                    });

                var message = $"A different item with {string.Join(" and ", valuesDisplay)} already exists.";
                return new(false, message, propViewModels.Select(p => new ValidationIssue(p.Name, message)));
            }
        }

        return null;
    }
}
