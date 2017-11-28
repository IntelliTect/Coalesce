using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Models;

namespace IntelliTect.Coalesce
{
    public interface IDataSource<T> where T : class
    {
        Task<(T item, IncludeTree includeTree)> GetItemAsync(object id);
        Task<TDto> GetMappedItemAsync<TDto>(object id) where TDto : IClassDto<T, TDto>, new();

        Task<(ListResult<T> list, IncludeTree includeTree)> GetListAsync();
        Task<ListResult<TDto>> GetMappedListAsync<TDto>() where TDto : IClassDto<T, TDto>, new();

        Task<int> GetCountAsync();
    }
}