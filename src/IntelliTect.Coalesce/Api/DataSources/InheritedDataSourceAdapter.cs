using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce;

/// <summary>
/// Adapts an <see cref="IDataSource{T}"/> that serves a base type
/// so that it can be used to retrieve individual items of a derived type.
/// This is used by the <see cref="Api.DataSources.DataSourceFactory"/>
/// when a data source declared on a base type in the hierarchy is resolved for a derived type.
/// List and count operations are not supported through inherited data sources
/// because they cannot correctly apply type-specific filtering.
/// </summary>
/// <typeparam name="TDerived">The derived entity type that needs to be served.</typeparam>
/// <typeparam name="TBase">The base entity type that the inner data source serves.</typeparam>
internal class InheritedDataSourceAdapter<TDerived, TBase> : IDataSource<TDerived>
    where TDerived : class, TBase
    where TBase : class
{
    private readonly IDataSource<TBase> _inner;
    private readonly ClassSecurityInfo _baseTypeSecurityInfo;
    private readonly Func<ClaimsPrincipal> _userAccessor;

    public InheritedDataSourceAdapter(
        IDataSource<TBase> inner,
        ClassSecurityInfo baseTypeSecurityInfo,
        Func<ClaimsPrincipal> userAccessor)
    {
        _inner = inner;
        _baseTypeSecurityInfo = baseTypeSecurityInfo;
        _userAccessor = userAccessor;
    }

    public async Task<ItemResult<TDerived>> GetItemAsync(object id, IDataSourceParameters parameters)
    {
        if (!_baseTypeSecurityInfo.IsReadAllowed(_userAccessor()))
        {
            return new ItemResult<TDerived>(wasSuccessful: false,
                message: $"Read access to {typeof(TBase).Name} is not authorized.");
        }

        var result = await _inner.GetItemAsync(id, parameters);

        if (!result.WasSuccessful || result.Object == null)
        {
            return new ItemResult<TDerived>(result);
        }

        if (result.Object is not TDerived derived)
        {
            return new ItemResult<TDerived>(wasSuccessful: false,
                message: $"Item with ID {id} is not a {typeof(TDerived).Name}.");
        }

        return new ItemResult<TDerived>(result, derived);
    }

    public async Task<ItemResult<TDto>> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
        where TDto : class, IResponseDto<TDerived>, new()
    {
        if (!_baseTypeSecurityInfo.IsReadAllowed(_userAccessor()))
        {
            return new ItemResult<TDto>(wasSuccessful: false,
                message: $"Read access to {typeof(TBase).Name} is not authorized.");
        }

        // TDto also implements IResponseDto<TBase> due to DTO inheritance hierarchy mirroring entities.
        // Invoke via reflection since the compiler can't prove this constraint.
        var method = typeof(IDataSource<TBase>)
            .GetMethod(nameof(GetMappedItemAsync))!
            .MakeGenericMethod(typeof(TDto));

        var task = (Task)method.Invoke(_inner, [id, parameters])!;
        await task;

        var result = (ItemResult<TDto>)task.GetType().GetProperty("Result")!.GetValue(task)!;

        if (!result.WasSuccessful || result.Object == null)
        {
            return result;
        }

        if (result.Object is not TDto dto)
        {
            return new ItemResult<TDto>(wasSuccessful: false,
                message: $"Item with ID {id} is not a {typeof(TDerived).Name}.");
        }

        return new ItemResult<TDto>(result, dto);
    }

    public Task<ListResult<TDerived>> GetListAsync(IListParameters parameters)
        => throw ThrowNotSupported();

    public Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
        where TDto : class, IResponseDto<TDerived>, new()
        => throw ThrowNotSupported();

    public Task<ItemResult<int>> GetCountAsync(IFilterParameters parameters)
        => throw ThrowNotSupported();

    private NotSupportedException ThrowNotSupported([System.Runtime.CompilerServices.CallerMemberName] string? caller = null)
    {
        return new NotSupportedException(
            $"{caller} is not supported through an inherited data source. " +
            $"The data source serves {typeof(TBase).Name} but was resolved for {typeof(TDerived).Name}. " +
            $"Declare a data source directly on {typeof(TDerived).Name} to support list/count operations.");
    }
}
