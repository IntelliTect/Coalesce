using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{
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
        public ClaimsPrincipal? User => Context.User;

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
        /// This is not recommended, as it can cause a client to recieve unexpected results.
        /// </summary>
        public IDataSource<T>? OverridePostSaveResultDataSource { get; protected set; }

        /// <summary>
        /// If set, this data source will be used in place of the supplied data source
        /// when reloading an object after a delete operation has completed.
        /// This is not recommended, as it can cause a client to recieve unexpected results.
        /// </summary>
        public IDataSource<T>? OverridePostDeleteResultDataSource { get; protected set; }

        /// <summary>
        /// If true, Coalesce will perform validation of incoming data using <see cref="ValidationAttribute"/>s
        /// present on your models during save operations (in <see cref="ValidateDto(SaveKind, IClassDto{T})"/>).
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
            where TDto : class, IClassDto<T>, new()
        {
            var dtoClassViewModel = Context.ReflectionRepository.GetClassViewModel<TDto>()!;
            var dtoPkInfo = dtoClassViewModel.PrimaryKey ?? throw new InvalidOperationException("Data sources cannot save items that lack a PK");

            object? idValue = dtoPkInfo.PropertyInfo.GetValue(incomingDto);

            if (ClassViewModel.PrimaryKey?.DatabaseGenerated == DatabaseGeneratedOption.None)
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

                var (existingItem, _) = await (OverrideFetchForUpdateDataSource ?? dataSource).GetItemAsync(idValue, parameters);
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
                : idValue!.Equals(Activator.CreateInstance(dtoClassViewModel.PrimaryKey.Type.TypeInfo)))
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
        protected virtual async Task<(ItemResult<T> Item, IncludeTree? IncludeTree)> FetchObjectAfterSaveAsync(IDataSource<T> dataSource, IDataSourceParameters parameters, T item)
        {
            var newItemId = ClassViewModel.PrimaryKey!.PropertyInfo.GetValue(item)!;
            var ds = (OverridePostSaveResultDataSource ?? dataSource);
            var result = await ds.GetItemAsync(newItemId, parameters);

            if (result.Item.Object != null && ds is IResultTransformer<T> transformer)
            {
                await transformer.TransformResultsAsync(Array.AsReadOnly(new[] { result.Item.Object }), parameters);
            }

            return result;
        }

        /// <summary>
        /// Maps the incoming DTO's properties to the item that will be saved to the database.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="item">The item that will be saved to the database.</param>
        /// <param name="dto">The incoming item from the client.</param>
        /// <param name="parameters">The additional parameters sent by the client.</param>
        protected virtual T MapIncomingDto<TDto>(SaveKind kind, T? item, TDto dto, IDataSourceParameters parameters)
            where TDto : class, IClassDto<T>, new()
        {
            var context = new MappingContext(User, parameters);
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
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="dto">The incoming item from the client.</param>
        /// <returns></returns>
        public virtual ItemResult ValidateDto(SaveKind kind, IClassDto<T> dto)
        {
            if (!ValidateAttributesForSaves) return true;
            return ItemResult.FromValidation(dto, deep: false, forceRequired: kind == SaveKind.Create, Context.ServiceProvider);
        }

        /// <summary>
        /// Code to run before committing a save to the database.
        /// Any changes made to the properties of <c>item</c> will be persisted to the database.
        /// The return a failure result will halt the save operation and return any associated message to the client.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
        /// If kind == SaveKind.Create, this will be null.</param>
        /// <param name="item">An entity instance with its properties set to incoming, new values.</param>
        /// <returns>An ItemResult potentially indicating failure, upon which the save operation will halt without persisting changes.</returns>
        public virtual ItemResult BeforeSave(SaveKind kind, T? oldItem, T item) => true;

        /// <summary>
        /// Code to run before committing a save to the database.
        /// Any changes made to the properties of <c>item</c> will be persisted to the database.
        /// The return a failure result will halt the save operation and return any associated message to the client.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
        /// If kind == SaveKind.Create, this will be null.</param>
        /// <param name="item">An entity instance with its properties set to incoming, new values.</param>
        /// <returns>An ItemResult potentially indicating failure, upon which the save operation will halt without persisting changes.</returns>
        public virtual Task<ItemResult> BeforeSaveAsync(SaveKind kind, T? oldItem, T item) => Task.FromResult(BeforeSave(kind, oldItem, item));

        /// <summary>
        /// Save the specified item to the database.
        /// </summary>
        /// <typeparam name="TDto">
        ///     The type of the DTO that contains the data to be saved,
        ///     and the type of DTO that the caller expects to recieve the results in.
        /// </typeparam>
        /// <param name="incomingDto">The DTO containing the properties to update.</param>
        /// <param name="dataSource">The data source that will be used when loading the item to be updated.</param>
        /// <param name="parameters">The parameters to be passed to the data source when loading the item.</param>
        /// <returns>A result indicating success or failure, as well as an up-to-date copy of the object being saved.</returns>
        public virtual async Task<ItemResult<TDto?>> SaveAsync<TDto>(
            TDto incomingDto,
            IDataSource<T> dataSource,
            IDataSourceParameters parameters
        )
            where TDto : class, IClassDto<T>, new()
        {
            (SaveKind kind, object? idValue) = await DetermineSaveKindAsync(incomingDto, dataSource, parameters);

            T? originalItem = null;
            T? item = null;
            IncludeTree? includeTree;

            if (kind == SaveKind.Update)
            {
                // Primary Key was defined. This object should exist in the database.
                var (existingItem, _) = await (OverrideFetchForUpdateDataSource ?? dataSource).GetItemAsync(idValue!, parameters);
                if (!existingItem.WasSuccessful)
                {
                    return new ItemResult<TDto?>(existingItem);
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
                return new ItemResult<TDto?>(validateDto);
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
                return new ItemResult<TDto?>(beforeSave);
            }

            await ExecuteSaveAsync(kind, originalItem, item);

            // Pull the object to get any changes.
            ItemResult<T> newItem;
            (newItem, includeTree) = await FetchObjectAfterSaveAsync(dataSource, parameters, item);
            if (!newItem.WasSuccessful)
            {
                return $"The item was saved, but could not be loaded with the requested data source: {newItem.Message}";
            }

            item = newItem.Object ?? throw new InvalidOperationException(
                $"Expected {nameof(ItemResult)}{nameof(ItemResult<T>.Object)} to be non-null when {nameof(FetchObjectAfterSaveAsync)} returns success.");

            // Call the AfterSave method to allow the user to
            // modify the returned object, the include tree,
            // or signal an error.
            var afterSave = AfterSave(kind, originalItem, ref item, ref includeTree);
            if (afterSave == null)
            {
                throw new InvalidOperationException("Received null from result of AfterSave. Expected an ItemResult<TDto>.");
            }
            else if (!afterSave.WasSuccessful)
            {
                return new ItemResult<TDto?>(afterSave);
            }

            // If the user nulled out the item in their AfterSave,
            // they don't want to send the item back with the save.
            // This is fine - we won't try to map it if its null.
            if (item == null)
            {
                return true;
            }

            return new ItemResult<TDto?>(
                item.MapToDto<T, TDto>(new MappingContext(User, parameters.Includes), includeTree)
            );
        }

        /// <summary>
        /// Executes the save action against the database and persists the changes.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
        /// If kind == SaveKind.Create, this will be null.</param>
        /// <param name="item">An entity instance with its properties set to incoming, new values.</param>
        public abstract Task ExecuteSaveAsync(SaveKind kind, T? oldItem, T item);

        /// <summary>
        /// Code to run after a save has been committed to the database.
        /// Allows any cleanup code to run, as well as modification of the object that will be returned to the client.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
        /// If kind == SaveKind.Create, this will be null.</param>
        /// <param name="item">
        /// A fresh copy of the modified item retrieved from the database,
        /// complete with any relations that were included as a result of being loaded 
        /// from the dataSource that was specified by the client.
        /// This ref parameter may have its value changed in order to send a modified object to the client.
        /// Set to null to return no object to the client.
        /// </param>
        /// <param name="includeTree">
        /// The includeTree that will be used to map the updatedItem for serialization and transmission to the client.
        /// The includeTree is obtained from the dataSource that was used to load updatedItem.
        /// This ref parameter may have its value changed to send a different object structure to the client.
        /// </param>
        /// <returns>An ItemResult potentially indicating failure. A failure response will be returned immediately without the updatedItem attached to the response.</returns>
        public virtual ItemResult AfterSave(SaveKind kind, T? oldItem, ref T item, ref IncludeTree? includeTree) => true;

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
            where TDto : class, IClassDto<T>, new()
        {
            var (existingItem, _) = await (OverrideFetchForDeleteDataSource ?? dataSource).GetItemAsync(id, parameters);
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
            await ExecuteDeleteAsync(item);



            // Pull the object to see if it can still be seen by the user.
            // If so, the operation was a soft delete and the user is allowed to see soft-deleted items.
            var postDeleteDs = OverridePostDeleteResultDataSource ?? dataSource;
            var (postDeleteGetResult, includeTree) = await postDeleteDs.GetItemAsync(id, parameters);

            var deletedItem = postDeleteGetResult.Object;


            if (deletedItem == null)
            {
                // If the item was not retrieved, it was actually deleted.
                // Don't return the item to the client since it no longer exists (in the context of the applicable data source, anyway).

                AfterDelete(ref item, ref includeTree);
                return true;
            }
            else
            {
                // If the item WAS retrieved, it was soft deleted and still visible to the current user through a data source.
                // We return it to the client to signal two things:
                // 1) Update the item on which delete was called with the new object.
                // 2) Don't remove the item from its parent collections on the client, since it still exists through the requested data source.

                // Allow the user to override this behavior using the AfterDelete method.
                AfterDelete(ref deletedItem, ref includeTree);

                // If the user nulled out the item, they don't want to send it back to the client.
                // This is fine, and is documented behavior.
                if (deletedItem == null)
                {
                    return true;
                }

                if (postDeleteDs is IResultTransformer<T> transformer)
                {
                    await transformer.TransformResultsAsync(Array.AsReadOnly(new[] { deletedItem }), parameters);
                }

                return new ItemResult<TDto?>(
                    deletedItem.MapToDto<T, TDto>(new MappingContext(User, parameters.Includes), includeTree)
                );
            }
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
        /// This ref parameter may have its value changed in order to send a modified object back to the client.
        /// Set to null to return no object to the client.
        /// </param>
        /// <param name="includeTree">
        /// The includeTree that will be used to map the deleted item for serialization and transmission to the client.
        /// The includeTree is obtained from the dataSource that was used to load the deleted item.
        /// This ref parameter may have its value changed to send a different object structure to the client.
        /// In the case where the deleted item was not retrieved from the database, changing this will have no effect.
        /// </param>
        public virtual void AfterDelete(ref T item, ref IncludeTree? includeTree) { }

        #endregion
    }
}
