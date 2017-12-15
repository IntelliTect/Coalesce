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
        Task<ItemResult<TDto>> SaveAsync<TDto>(
            TDto incomingDto,
            IDataSourceParameters parameters,
            IDataSource<T> dataSource
        ) where TDto : IClassDto<T>, new();

        Task<ItemResult> DeleteAsync(object id);

    }
}
