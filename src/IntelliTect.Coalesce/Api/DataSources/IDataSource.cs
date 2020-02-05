using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Models;

namespace IntelliTect.Coalesce
{

    public interface IDataSource<T>
        where T : class, new()
    {
        Task<(ItemResult<T> Item, IncludeTree IncludeTree)> GetItemAsync(object id, IDataSourceParameters parameters);

        Task<ItemResult<TDto>> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
            where TDto : class, IClassDto<T>, new();

        Task<(ListResult<T> List, IncludeTree IncludeTree)> GetListAsync(IListParameters parameters);

        Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
            where TDto : class, IClassDto<T>, new();

        Task<ItemResult<int>> GetCountAsync(IFilterParameters parameters);
    }
}