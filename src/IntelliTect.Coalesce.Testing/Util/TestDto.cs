using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;

#nullable enable

namespace IntelliTect.Coalesce.Testing.Util;

public class TestDto<T> : IClassDto<T>
    where T : class
{
    public TestDto() { }

    public TestDto(long? id, Action<T>? mapTo = null)
    {
        Id = id;
        MapTo = mapTo;
    }

    public long? Id { get; set; }

    public T? SourceEntity { get; private set; }
    public IncludeTree? SourceIncludeTree { get; private set; }

    public Action<TestDto<T>, T>? MapFrom { get; set; }
    public Action<T>? MapTo { get; set; }

    void IResponseDto<T>.MapFrom(T obj, IMappingContext context, IncludeTree? tree)
    {
        SourceEntity = obj;
        SourceIncludeTree = tree;
        MapFrom?.Invoke(this, obj);
    }

    void IParameterDto<T>.MapTo(T obj, IMappingContext context)
    {
        MapTo?.Invoke(obj);
    }
}

public class TestSparseDto<T> : TestDto<T>, ISparseDto
    where T : class
{
    public ISet<string> ChangedProperties { get; } = new HashSet<string>();
}
