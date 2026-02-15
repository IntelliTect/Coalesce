using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Core.Interfaces;

namespace IntelliTect.Coalesce.Testing.Util;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PropertyViewModelDataAttribute : Attribute, IDataSourceAttribute
{
    private readonly Type targetClass;
    private readonly string propName;
    private readonly object[] inlineData;

    protected bool reflection = true;
    protected bool symbol = true;

    public PropertyViewModelDataAttribute(Type targetClass, string propName, params object[] additionalInlineData)
    {
        this.targetClass = targetClass;
        this.propName = propName;
        this.inlineData = additionalInlineData;
    }

    public bool SkipIfEmpty { get; set; }

    public async IAsyncEnumerable<Func<Task<object[]>>> GetDataRowsAsync(DataGeneratorMetadata dataGeneratorMetadata)
    {
        if (reflection)
        {
            yield return () => Task.FromResult(new[] {
                new PropertyViewModelData(targetClass, propName, typeof(ReflectionClassViewModel))
            }.Concat(inlineData).ToArray());
        }

        if (symbol)
        {
            yield return () => Task.FromResult(new[] {
                new PropertyViewModelData(targetClass, propName, typeof(SymbolClassViewModel))
            }.Concat(inlineData).ToArray());
        }

        await Task.CompletedTask;
    }
}

public class PropertyViewModelDataAttribute<T> : PropertyViewModelDataAttribute
{
    public PropertyViewModelDataAttribute(string propName, params object[] additionalInlineData) : base(typeof(T), propName, additionalInlineData)
    {
    }
}

public class ReflectionPropertyViewModelDataAttribute<T> : PropertyViewModelDataAttribute<T>
{
    public ReflectionPropertyViewModelDataAttribute(string propName, params object[] additionalInlineData) : base(propName, additionalInlineData)
    {
        symbol = false;
    }
}

public class SymbolPropertyViewModelDataAttribute<T> : PropertyViewModelDataAttribute<T>
{
    public SymbolPropertyViewModelDataAttribute(string propName, params object[] additionalInlineData)
        : base(propName, additionalInlineData)
    {
        reflection = false;
    }
}
