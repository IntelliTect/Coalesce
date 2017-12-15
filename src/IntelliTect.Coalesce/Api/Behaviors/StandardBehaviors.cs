using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Data;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{
    public class StandardBehaviors<T, TContext> : StandardCrudStrategy<T, TContext>, IBehaviors<T>
        where T : class, new()
        where TContext : DbContext
    {

        public StandardBehaviors(CrudContext<TContext> context) : base(context)
        {
        }

        /// <summary>
        /// Get a DbSet representing the type handled by these behaviors.
        /// </summary>
        /// <returns></returns>
        protected virtual DbSet<T> GetDbSet() => Db.Set<T>();

        /// <summary>
        /// Retrieve a item with the specified key from the database without any relations/includes.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T GetItem(object id) => GetDbSet().FindItem(id);

        #region Delete

        /// <summary>
        /// Delete the item with the specified key from the database.
        /// </summary>
        /// <param name="id">The primary key of the item to delete.</param>
        /// <returns>An ItemResult indicating success/failure of the operation.</returns>
        public virtual async Task<ItemResult> DeleteAsync(object id)
        {
            T item = GetItem(id);
            if (item == null)
            {
                return $"Item with ID {id} was not found.";
            }

            var beforeDelete = BeforeDelete(item);
            if (!beforeDelete?.WasSuccessful ?? true)
            {
                return beforeDelete;
            }

            await ExecuteDeleteAsync(item);

            AfterDelete(item);

            return true;
        }

        /// <summary>
        /// Code to run before committing the delete operation to the database.
        /// If a failure result is returned, the delete operation will not execute.
        /// This method is called by DeleteAsync.
        /// This may be used to implement row-level 
        /// </summary>
        /// <param name="item">The item being deleted.</param>
        /// <returns>An ItemResult that, if indicating failure, will halt the delete operation.</returns>
        public virtual ItemResult BeforeDelete(T item) => true;

        /// <summary>
        /// Executes the delete action against the database and saves the change.
        /// This may be overridden to change what action is actually performed against the database 
        /// on deletion of an item (e.g. setting a deleted flag instead of deleting the row).
        /// </summary>
        /// <param name="dbSet">The </param>
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
        /// <param name="item">The item that was deleted.</param>
        public virtual void AfterDelete(T item) { }

        #endregion


        #region Save

        /// <summary>
        /// Save the specified item to the database.
        /// </summary>
        /// <typeparam name="TDto">
        ///     The type of the DTO that contains the data to be saved,
        ///     and the type of DTO that the caller expects to recieve the results in.
        /// </typeparam>
        /// <param name="incomingDto">The DTO that contains the data that needs to be saved.</param>
        /// <param name="parameters">Parameters pertaining to the dataSource that were provided by the client.</param>
        /// <param name="dataSource">
        /// The dataSource requested by the client that should be used to load the object that is returned to the client.
        /// This dataSource will NOT be used to load the initial object 
        /// </param>
        /// <returns></returns>
        public virtual async Task<ItemResult<TDto>> SaveAsync<TDto>(
            TDto incomingDto,
            IDataSourceParameters parameters,
            IDataSource<T> dataSource
        )
            where TDto : IClassDto<T>, new()
        {
            var dbSet = GetDbSet();
            var includes = parameters.Includes;

            T item = null;
            IncludeTree includeTree = null;
            
            (SaveKind kind, object idValue) = DetermineSaveKind(incomingDto);
            if (kind == SaveKind.Create)
            {
                item = new T();
                dbSet.Add(item);
            }
            else
            {
                // Primary Key was defined. This object should exist in the database.
                item = GetItem(idValue);
                if (item == null)
                {
                    return $"Item with {ClassViewModel.PrimaryKey.Name} = {idValue} not found.";
                }
            }

            // Create a shallow copy.
            var originalItem = item.Copy();

            // Set all properties on the DB-mapped object to the incoming values.
            incomingDto.MapToEntity(item, new MappingContext(User, includes));

            // Allow interception of the save.
            var beforeSave = BeforeSave(kind, originalItem, item);
            if (!beforeSave?.WasSuccessful ?? true) return new ItemResult<TDto>(beforeSave);

            await Db.SaveChangesAsync();

            // Pull the object to get any changes.
            var newItemId = ClassViewModel.PrimaryKey.PropertyInfo.GetValue(item);
            (item, includeTree) = await dataSource.GetItemAsync(newItemId, parameters);

            // Call the AfterSave method to allow the user to
            // modify the returned object, the include tree,
            // or signal an error.
            var afterSave = AfterSave(kind, originalItem, ref item, ref includeTree);
            if (!afterSave?.WasSuccessful ?? true) return new ItemResult<TDto>(afterSave);

            // If the user nulled out the item in their AfterSave,
            // they don't want to send the item back with the save.
            // This is fine - we won't try to map it if its null.
            if (item == null) return true;

            var result = new ItemResult<TDto>(true,
                item.MapToDto<T, TDto>(new MappingContext(User, includes), includeTree)
            );

            return result;
        }

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

        /// <summary>
        /// Code to run before committing a save to the database.
        /// Any changes made to the properties of <c>updatedItem</c> will be persisted to the database.
        /// The return a failure result will halt the save operation and return any associated message to the client.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="originalItem">A shallow copy of the original item as it was retrieved from the database.</param>
        /// <param name="updatedItem">A DbContext-tracked entity with its properties set to incoming, new values.</param>
        /// <returns>An ItemResult potentially indicating failure, upon which the save operation will halt without persisting changes.</returns>
        public virtual ItemResult BeforeSave(SaveKind kind, T originalItem, T updatedItem) => true;

        /// <summary>
        /// Code to run after a save has been committed to the database.
        /// Allows any cleanup code to run, as well as modification of the object that will be returned to the client.
        /// </summary>
        /// <param name="kind">Descriminator between a create and a update operation.</param>
        /// <param name="originalItem">A shallow copy of the original item as it was retrieved from the database.</param>
        /// <param name="updatedItem">
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
        public virtual ItemResult AfterSave(SaveKind kind, T originalItem, ref T updatedItem, ref IncludeTree includeTree) => true;

        #endregion

    }

    public enum SaveKind
    {
        Create,
        Update,
    }
}
