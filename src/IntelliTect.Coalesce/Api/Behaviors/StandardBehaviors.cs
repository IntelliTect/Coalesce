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
    public class StandardBehaviors<T, TContext> : IBehaviors<T>
        where T : class, new()
        where TContext : DbContext
    {

        /// <summary>
        /// Contains contextual information about the request for data being served.
        /// </summary>
        public CrudContext<TContext> Context { get; }

        /// <summary>
        /// The DbContext to be used for this request.
        /// </summary>
        public TContext Db => Context.DbContext;

        /// <summary>
        /// The user making the request for data.
        /// </summary>
        public ClaimsPrincipal User => Context.User;

        /// <summary>
        /// A ClassViewModel representing the type T that is provided by this data source.
        /// </summary>
        public ClassViewModel ClassViewModel { get; protected set; }

        public StandardBehaviors(CrudContext<TContext> context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ClassViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
        }

        protected virtual DbSet<T> GetDbSet() => Db.Set<T>();


        public virtual T GetItem(DbSet<T> dbSet, object id) => dbSet.FindItem(id);

        #region Delete

        public virtual async Task<ItemResult> DeleteAsync(object id)
        {
            var dbSet = GetDbSet();

            T item = GetItem(dbSet, id);
            if (item == null)
            {
                return $"Item with ID {id} was not found.";
            }

            var beforeDelete = BeforeDelete(item);
            if (!beforeDelete?.WasSuccessful ?? true)
            {
                return beforeDelete;
            }

            dbSet.Remove(item);
            await Db.SaveChangesAsync();

            AfterDelete(item);

            return true;
        }

        public virtual ItemResult BeforeDelete(T item) => true;

        public virtual void AfterDelete(T item) { }

        #endregion


        #region Save

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

            // IsNullable handles nullable value types, and reference types (mainly strings).
            // !IsNullable handles non-Nullable<T> value types.
            (SaveKind kind, object idValue) = DetermineSaveKind(incomingDto);
            if (kind == SaveKind.Create)
            {
                item = new T();
                dbSet.Add(item);
            }
            else
            {
                // Primary Key was defined. This object should exist in the database.
                item = GetItem(dbSet, idValue);
                if (item == null)
                {
                    return $"Item with {ClassViewModel.PrimaryKey.Name} = {idValue} not found.";
                }
            }

            // Create a shallow copy.
            var originalItem = item.Copy();

            incomingDto.MapToEntity(item, new MappingContext(User, includes));

            var beforeSave = BeforeSave(kind, originalItem, item);
            if (!beforeSave?.WasSuccessful ?? true) return new ItemResult<TDto>(beforeSave);

            await Db.SaveChangesAsync();

            // Pull the object to get any changes.
            var newItemId = ClassViewModel.PrimaryKey.PropertyInfo.GetValue(item);
            (item, includeTree) = await dataSource.GetItemAsync(newItemId, parameters);

            // Call the AfterSave method to support special cases.
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

        public virtual ItemResult BeforeSave(SaveKind kind, T originalItem, T updatedItem) => true;

        public virtual ItemResult AfterSave(SaveKind kind, T originalItem, ref T updatedItem, ref IncludeTree includeTree) => true;

        #endregion




    }

    public enum SaveKind
    {
        Create,
        Update,
    }
}
