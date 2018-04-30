using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{
    public interface IBehaviors<T>
        where T : class, new()
    {
        /// <summary>
        /// Given the incoming DTO, discern from its properties whether the save action
        /// is a create or update action. If an update action, also provide the key of the existing item to be updated.
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="incomingDto"></param>
        /// <returns></returns>
        (SaveKind Kind, object IncomingKey) DetermineSaveKind<TDto>(TDto incomingDto)
            where TDto : IClassDto<T>, new();

        /// <summary>
        /// Save an item to the database.
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="incomingDto">The DTO containing the properties to update.</param>
        /// <param name="dataSource">The data source that will be used when loading the item to be updated.</param>
        /// <param name="parameters">The parameters to be passed to the data source when loading the item.</param>
        /// <returns>A result indicating success or failure, as well as an up-to-date copy of the object being saved.</returns>
        Task<ItemResult<TDto>> SaveAsync<TDto>(
            TDto incomingDto,
            IDataSource<T> dataSource,
            IDataSourceParameters parameters
        ) where TDto : IClassDto<T>, new();


        /// <summary>
        /// Delete an item from the database.
        /// </summary>
        /// <param name="id">The primary key of the object to delete.</param>
        /// <param name="dataSource">The data source that will be used when loading the item to be deleted.</param>
        /// <param name="parameters">The parameters to be passed to the data source when loading the item.</param>
        /// <returns>A result indicating success or failure, 
        /// potentially including an up-to-date copy of the item being deleted if the delete action is non-destructive.</returns>
        Task<ItemResult<TDto>> DeleteAsync<TDto>(
            object id,
            IDataSource<T> dataSource,
            IDataSourceParameters parameters
        ) where TDto : IClassDto<T>, new();
    }
}
