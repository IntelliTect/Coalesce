using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IntelliTect.Coalesce.Tests.Util;

internal class PropertyViewModelDataAttribute : Xunit.Sdk.DataAttribute
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

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        if (reflection) yield return new[] {
            new PropertyViewModelData(targetClass, propName, typeof(ReflectionClassViewModel))
        }.Concat(inlineData).ToArray();

        if (symbol) yield return new[] {
            new PropertyViewModelData(targetClass, propName, typeof(SymbolClassViewModel))
        }.Concat(inlineData).ToArray();
    }
}

internal class PropertyViewModelDataAttribute<T> : PropertyViewModelDataAttribute
{
    public PropertyViewModelDataAttribute(string propName, params object[] additionalInlineData) : base(typeof(T), propName, additionalInlineData)
    {
    }
}

internal class ReflectionPropertyViewModelDataAttribute<T> : PropertyViewModelDataAttribute<T>
{
    public ReflectionPropertyViewModelDataAttribute(string propName, params object[] additionalInlineData) : base(propName, additionalInlineData)
    {
        symbol = false;
    }
}

internal class SymbolPropertyViewModelDataAttribute<T> : PropertyViewModelDataAttribute<T>
{
    public SymbolPropertyViewModelDataAttribute(string propName, params object[] additionalInlineData)
        : base(propName, additionalInlineData)
    {
        reflection = false;
    }
}
