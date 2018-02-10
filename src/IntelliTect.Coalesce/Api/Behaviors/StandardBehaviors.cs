using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{
    public class StandardBehaviors<T, TContext> : StandardCrudStrategy<T, TContext>, IEntityFrameworkBehaviors<T, TContext>
        where T : class, new()
        where TContext : DbContext
    {

        /// <summary>
        /// If set, this data source will be used in place of the supplied data source
        /// when retrieving an object that will be updated in a Save operation.
        /// </summary>
        public IDataSource<T> OverrideFetchForUpdateDataSource { get; protected set; }

        /// <summary>
        /// If set, this data source will be used in place of the supplied data source
        /// when retrieving an object that will be deleted.
        /// </summary>
        public IDataSource<T> OverrideFetchForDeleteDataSource { get; protected set; }

        /// <summary>
        /// If set, this data source will be used in place of the supplied data source
        /// when reloading an object after a save operation has completed.
        /// This is not recommended, as it can cause a client to recieve unexpected results.
        /// </summary>
        public IDataSource<T> OverridePostSaveResultDataSource { get; protected set; }

        /// <summary>
        /// If set, this data source will be used in place of the supplied data source
        /// when reloading an object after a delete operation has completed.
        /// This is not recommended, as it can cause a client to recieve unexpected results.
        /// </summary>
        public IDataSource<T> OverridePostDeleteResultDataSource { get; protected set; }


        public StandardBehaviors(CrudContext<TContext> context) : base(context)
        {
        }

        /// <summary>
        /// Get a DbSet representing the type handled by these behaviors.
        /// </summary>
        /// <returns></returns>
        protected virtual DbSet<T> GetDbSet() => Db.Set<T>();

        /// <summary>
        /// From the incoming DTO, determines if the operation should be a create or update operation.
        /// Also discerns the primary key of the object that is being operated upon.
        /// </summary>
        /// <typeparam name="TDto">The type of the incoming DTO.</typeparam>
        /// <param name="incomingDto">The incoming DTO to obtain a primary key from.</param>
        /// <returns>A SaveKind indicating either Create or Update, 
        /// and the value of the primary key that can be used for database lookups.</returns>
        public virtual (SaveKind Kind, object IncomingKey) DetermineSaveKind<TDto>(TDto incomingDto)
            where TDto : IClassDto<T>, new()
        {
            var dtoClassViewModel = ReflectionRepository.Global.GetClassViewModel<TDto>();
            object idValue = dtoClassViewModel.PrimaryKey.PropertyInfo.GetValue(incomingDto);

            // IsNullable handles nullable value types, and reference types (mainly strings).
            // !IsNullable handles non-Nullable<T> value types.
            if (dtoClassViewModel.PrimaryKey.Type.IsNullable
                ? idValue == null
                : idValue.Equals(Activator.CreateInstance(dtoClassViewModel.PrimaryKey.Type.TypeInfo)))
            {
                return (SaveKind.Create, null);
            }
            else
            {
                return (SaveKind.Update, idValue);
            }
        }

        #region Save

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
        public virtual async Task<ItemResult<TDto>> SaveAsync<TDto>(
            TDto incomingDto,
            IDataSource<T> dataSource,
            IDataSourceParameters parameters
        )
            where TDto : IClassDto<T>, new()
        {
            (SaveKind kind, object idValue) = DetermineSaveKind(incomingDto);

            T originalItem = null;
            T item = null;
            IncludeTree includeTree = null;

            var includes = parameters.Includes;
            var dbSet = GetDbSet();

            if (kind == SaveKind.Create)
            {
                item = new T();
                dbSet.Add(item);
            }
            else
            {
                // Primary Key was defined. This object should exist in the database.
                var (existingItem, _) = await (OverrideFetchForUpdateDataSource ?? dataSource).GetItemAsync(idValue, parameters);
                if (!existingItem.WasSuccessful)
                {
                    return new ItemResult<TDto>(existingItem);
                }
                item = existingItem.Object;

                // Ensure that the entity is tracked.
                // We want to allow for item retrieval from data sources that build their query with .AsNoTracking().
                Db.Entry(item).State = EntityState.Unchanged;

                // Create a shallow copy.
                originalItem = item.Copy();
            }

            // Allow validation on the raw DTO before its been mapped.
            var validateDto = ValidateDto(kind, incomingDto);
            if (validateDto == null)
                throw new InvalidOperationException("Recieved null from result of ValidateDto. Expected an ItemResult.");
            if (!validateDto.WasSuccessful) return new ItemResult<TDto>(validateDto);

            // Set all properties on the DB-mapped object to the incoming values.
            MapIncomingDto(kind, item, incomingDto, parameters);

            // Allow interception of the save.
            var beforeSave = BeforeSave(kind, originalItem, item);
            if (beforeSave == null)
                throw new InvalidOperationException("Recieved null from result of BeforeSave. Expected an ItemResult.");
            if (!beforeSave.WasSuccessful) return new ItemResult<TDto>(beforeSave);

            await Db.SaveChangesAsync();

            // Pull the object to get any changes.
            var newItemId = ClassViewModel.PrimaryKey.PropertyInfo.GetValue(item);
            ItemResult<T> newItem;
            (newItem, includeTree) = await (OverridePostSaveResultDataSource ?? dataSource).GetItemAsync(newItemId, parameters);

            if (!newItem.WasSuccessful)
            {
                return $"The item was saved, but could not be loaded with the requested data source: {newItem.Message}";
            }

            item = newItem.Object;

            // Call the AfterSave method to allow the user to
            // modify the returned object, the include tree,
            // or signal an error.
            var afterSave = AfterSave(kind, originalItem, ref item, ref includeTree);
            if (afterSave == null)
                throw new InvalidOperationException("Recieved null from result of AfterSave. Expected an ItemResult<TDto>.");
            if (!afterSave.WasSuccessful) return new ItemResult<TDto>(afterSave);

            // If the user nulled out the item in their AfterSave,
            // they don't want to send the item back with the save.
            // This is fine - we won't try to map it if its null.
            if (item == null) return true;

            var result = new ItemResult<TDto>(
                item.MapToDto<T, TDto>(new MappingContext(User, includes), includeTree)
            );

            return result;
        }

        /// <summary>
        /// Maps the incoming DTO's properties to the item that will be saved to the database.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="item">The item that will be saved to the database.</param>
        /// <param name="dto">The incoming item from the client.</param>
        /// <param name="parameters">The </param>
        protected virtual void MapIncomingDto<TDto>(SaveKind kind, T item, TDto dto, IDataSourceParameters parameters) 
            where TDto : IClassDto<T>, new()
        {
            dto.MapToModel(item, new MappingContext(User, parameters.Includes));
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
        public virtual ItemResult ValidateDto(SaveKind kind, IClassDto<T> dto) => true;

        /// <summary>
        /// Code to run before committing a save to the database.
        /// Any changes made to the properties of <c>item</c> will be persisted to the database.
        /// The return a failure result will halt the save operation and return any associated message to the client.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="oldItem">A shallow copy of the original item as it was retrieved from the database.
        /// If kind == SaveKind.Create, this will be null.</param>
        /// <param name="item">A DbContext-tracked entity with its properties set to incoming, new values.</param>
        /// <returns>An ItemResult potentially indicating failure, upon which the save operation will halt without persisting changes.</returns>
        public virtual ItemResult BeforeSave(SaveKind kind, T oldItem, T item) => true;

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
        public virtual ItemResult AfterSave(SaveKind kind, T oldItem, ref T item, ref IncludeTree includeTree) => true;

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
        public virtual async Task<ItemResult<TDto>> DeleteAsync<TDto>(
            object id,
            IDataSource<T> dataSource,
            IDataSourceParameters parameters)
            where TDto : IClassDto<T>, new()
        {
            var (existingItem, _) = await (OverrideFetchForDeleteDataSource ?? dataSource).GetItemAsync(id, parameters);
            if (!existingItem.WasSuccessful)
            {
                return existingItem.Message;
            }

            var item = existingItem.Object;

            var beforeDelete = BeforeDelete(item);
            if (beforeDelete == null)
                throw new InvalidOperationException("Recieved null from result of BeforeDelete. Expected an ItemResult.");
            if (!beforeDelete.WasSuccessful) return new ItemResult<TDto>(beforeDelete);

            // Perform the delete operation against the database.
            // By default, this removes the item from its DbSet<> and calls SaveChanges().
            // This might be overridden to set a deleted flag on the object instead.
            await ExecuteDeleteAsync(item);

            

            // Pull the object to see if it can still be seen by the user.
            // If so, the operation was a soft delete and the user is allowed to see soft-deleted items.
            var (postDeleteGetResult, includeTree) = await (OverridePostDeleteResultDataSource ?? dataSource).GetItemAsync(id, parameters);

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

                return new ItemResult<TDto>(
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
        /// Executes the delete action against the database and saves the change.
        /// This may be overridden to change what action is actually performed against the database 
        /// on deletion of an item (e.g. setting a deleted flag instead of deleting the row).
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual Task ExecuteDeleteAsync(T item)
        {
            GetDbSet().Remove(item);
            return Db.SaveChangesAsync();
        }

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
        public virtual void AfterDelete(ref T item, ref IncludeTree includeTree) { }

        #endregion

    }

    public enum SaveKind
    {
        Create,
        Update,
    }
}
