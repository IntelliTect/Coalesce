using IntelliTect.Coalesce.DataAnnotations;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Models;

public abstract class SparseDto : ISparseDto
{
    private readonly HashSet<string> _changedProperties = new HashSet<string>();
    [InternalUse]
    ISet<string> ISparseDto.ChangedProperties => _changedProperties;

    protected void Changed(string propName) => _changedProperties.Add(propName);
    protected bool ShouldMapTo(string propName) => _changedProperties.Contains(propName);
}

public interface IGeneratedParameterDto<T> : IParameterDto<T>, ISparseDto
    where T : class
{
    public T MapToModelOrNew(T obj, IMappingContext context)
    {
        if (obj is null) return MapToNew(context);
        MapTo(obj, context);
        return obj;
    }
}

public interface IGeneratedResponseDto<T> : IResponseDto<T>
    where T : class
{
}

public interface ISparseDto
{
    ISet<string> ChangedProperties { get; }
}
