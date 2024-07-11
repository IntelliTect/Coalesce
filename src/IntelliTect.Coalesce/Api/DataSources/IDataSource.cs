using IntelliTect.Coalesce.Models;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{

    public interface IDataSource<T>
        where T : class
    {
        Task<ItemResult<T>> GetItemAsync(object id, IDataSourceParameters parameters);

        Task<ItemResult<TDto>> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
            where TDto : class, IResponseDto<T>, new();

        Task<ListResult<T>> GetListAsync(IListParameters parameters);

        Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
            where TDto : class, IResponseDto<T>, new();

        Task<ItemResult<int>> GetCountAsync(IFilterParameters parameters);
    }
}