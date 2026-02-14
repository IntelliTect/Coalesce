using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Core.Interfaces;

namespace IntelliTect.Coalesce.Tests.Util;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class ClassViewModelDataAttribute : Attribute, IDataSourceAttribute
{
    private readonly Type targetClass;
    private readonly object[] inlineData;

    protected bool reflection = true;
    protected bool symbol = true;

    public ClassViewModelDataAttribute(Type targetClass, params object[] additionalInlineData)
    {
        this.targetClass = targetClass;
        this.inlineData = additionalInlineData;
    }

    public bool SkipIfEmpty { get; set; }

    public async IAsyncEnumerable<Func<Task<object[]>>> GetDataRowsAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        if (reflection)
        {
            yield return () => Task.FromResult(new[] {
                new ClassViewModelData(targetClass, typeof(ReflectionClassViewModel))
            }.Concat(inlineData).ToArray());
        }

        if (symbol)
        {
            yield return () => Task.FromResult(new[] {
                new ClassViewModelData(targetClass, typeof(SymbolClassViewModel))
            }.Concat(inlineData).ToArray());
        }

        await Task.CompletedTask;
    }
}

internal class ClassViewModelDataAttribute<T> : ClassViewModelDataAttribute
{
    public ClassViewModelDataAttribute(params object[] additionalInlineData) : base(typeof(T), additionalInlineData)
    {
    }
}

internal class ReflectionClassViewModelDataAttribute : ClassViewModelDataAttribute
{
    public ReflectionClassViewModelDataAttribute(Type targetClass, params object[] additionalInlineData)
        : base(targetClass, additionalInlineData)
    {
        this.symbol = false;
    }
}

internal class SymbolClassViewModelDataAttribute : ClassViewModelDataAttribute
{
    public SymbolClassViewModelDataAttribute(Type targetClass, params object[] additionalInlineData)
        : base(targetClass, additionalInlineData)
    {
        this.reflection = false;
    }
}
