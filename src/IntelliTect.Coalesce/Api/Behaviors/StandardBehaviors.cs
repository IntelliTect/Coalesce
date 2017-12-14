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
    public class StandardBehaviors<T, TContext>
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

        public virtual ItemResult BeforeDelete(T item) => true;

        public virtual void AfterDelete(T item) { }

        public virtual async Task<ItemResult> DeleteAsync(object id)
        {
            var dbSet = GetDbSet();

            T item = GetItem(dbSet, id);
            if (item == null)
            {
                return $"Item with ID {id} was not found.";
            }

            var beforeDelete = BeforeDelete(item);
            if (!beforeDelete.WasSuccessful)
            {
                return beforeDelete;
            }

            dbSet.Remove(item);
            await Db.SaveChangesAsync();

            AfterDelete(item);

            return true;
        }

        #endregion
        #region Save


        /// <summary>
        /// Allows for overriding the mapper from Obj to DTO
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual TDto MapObjToDto<TDto>(T obj, string includes, IncludeTree tree = null)
            where TDto : IClassDto<T, TDto>, new()
        {
            //return Activator.CreateInstance(typeof(TDto), new object[] { obj, User, includes }) as TDto;
            var context = new MappingContext(User, includes);
            return Mapper<T, TDto>.ObjToDtoMapper(obj, context, tree);
        }

        /// <summary>
        /// Allows for overriding the mapper from DTO to Obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>        
        protected virtual void MapDtoToObj<TDto>(TDto dto, T obj, string includes)
            where TDto : IClassDto<T, TDto>, new()
        {
            //dto.Update(obj);
            var context = new MappingContext(User, includes);
            Mapper<T, TDto>.DtoToObjMapper(dto, obj, context);
        }


        public virtual async Task<ItemResult<TDto>> Save<TDto>(
            TDto incomingDto,
            IDataSourceParameters parameters,
            IDataSource<T> dataSource
        )
            where TDto : IClassDto<T, TDto>, new()
        {
            var dbSet = GetDbSet();
            var includes = parameters.Includes;

            var result = new ItemResult<TDto>();

            T item = null;

            // All properties on DTOs should be nullable.
            // We can expect, then, that a "create" scenario should have idValue == null.
            var dtoClassViewModel = ReflectionRepository.Global.GetClassViewModel<TDto>();
            object idValue = dtoClassViewModel.PrimaryKey.PropertyInfo.GetValue(incomingDto);

            if (idValue != null)
            {
                // Primary Key was defined. This object should exist in the database.
                item = GetItem(dbSet, idValue);
                if (item == null)
                {
                    return $"Item with {ClassViewModel.PrimaryKey.Name} = {idValue} not found.";
                }
            }
            else
            {
                item = new T();
                dbSet.Add(item);
            }

            // Create a shallow copy.
            var originalItem = item.Copy();

            // Allow the user to stop things from saving.
            IncludeTree includeTree = null;

            MapDtoToObj(incomingDto, item, includes);
            try
            {
                SetFingerprint(item);
                // Run validation in this controller
                var validateResult = Validate(originalItem, incomingDto, item);
                // Run validation from the POCO if it implements IValidatable
                if (typeof(IBeforeSave<T, TContext>).IsAssignableFrom(typeof(T)))
                {
                    var itemAsBeforeSave = item as IBeforeSave<T, TContext>;
                    validateResult.Merge(itemAsBeforeSave.BeforeSave(originalItem, Db, User, includes));
                }

                if (validateResult.WasSuccessful)
                {
                    await Db.SaveChangesAsync();

                    // Pull the object to get any changes.
                    var idString = ClassViewModel.PrimaryKey.PropertyInfo.GetValue(item);
                    var itemResult = await dataSource.GetItemAsync(idString, parameters);
                    item = itemResult.Item1;
                    includeTree = itemResult.Item2;

                    // Call the AfterSave method to support special cases.
                    var reloadItem = AfterSave(incomingDto, item, originalItem, Db);

                    // Call PostSave if the object has that.
                    if (typeof(IAfterSave<T, TContext>).IsAssignableFrom(typeof(T)))
                    {
                        var itemAsAfterSave = item as IAfterSave<T, TContext>;
                        itemAsAfterSave.AfterSave(originalItem, Db, User, includes);
                    }

                    if (reloadItem && returnObject)
                    {
                        itemResult = await dataSource.GetItemAsync(idString, parameters);
                        item = itemResult.Item1;
                        includeTree = itemResult.Item2;
                    }

                    result.WasSuccessful = true;
                }
                else
                {
                    result.WasSuccessful = false;
                    result.Message = validateResult.Message;
                    if (validateResult.ReturnObject != null)
                    {
                        result.Object = MapObjToDto(validateResult.ReturnObject, includes);
                    }
                }
            }
            catch (Exception ex)
            {
                result.WasSuccessful = false;
                result.Message = ex.Message;
            }

            // Get the key back.
            if (item != null)
            {
                result.Object = MapObjToDto<TDto>(item, includes, includeTree);
            }

            return result;
        }

#endregion




    }
}
