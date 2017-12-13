using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Models;

namespace IntelliTect.Coalesce
{

    public interface IDataSource<T> : IAuthorizable
        where T : class, new()
    {
        Task<(T item, IncludeTree includeTree)> GetItemAsync(object id, IDataSourceParameters parameters);
        Task<TDto> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
            where TDto : IClassDto<T, TDto>, new();

        Task<(ListResult<T> list, IncludeTree includeTree)> GetListAsync(IListParameters parameters);
        Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
            where TDto : IClassDto<T, TDto>, new();

        Task<int> GetCountAsync(IFilterParameters parameters);
    }
}