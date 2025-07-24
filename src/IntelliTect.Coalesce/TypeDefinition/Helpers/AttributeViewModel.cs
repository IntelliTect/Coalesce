using IntelliTect.Coalesce.Utilities;
using System;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.TypeDefinition;

public abstract class AttributeViewModel<TAttribute>
    where TAttribute : Attribute
{
    public ReflectionRepository ReflectionRepository { get; }

    public abstract TypeViewModel Type { get; }

    protected AttributeViewModel(ReflectionRepository? reflectionRepository)
    {
        ReflectionRepository = reflectionRepository ?? ReflectionRepository.Global;
    }

    public T? GetValue<T>(string valueName)
        where T : struct
    {
        var result = GetValue(valueName);
        if (result == null)
        {
            return null;
        }
        return new T?((T)result);
    }

    public string? GetValue(Expression<Func<TAttribute, string?>> propertyExpression)
    {
        return GetValue(propertyExpression.GetExpressedProperty().Name) as string;
    }

    public T? GetValue<T>(Expression<Func<TAttribute, T?>> propertyExpression)
        where T : class
    {
        return GetValue(propertyExpression.GetExpressedProperty().Name) as T;
    }

    public T? GetValue<T>(
        Expression<Func<TAttribute, T>> propertyExpression,
        Expression<Func<TAttribute, bool>>? hasValueExpression = null
    )
        where T : struct
    {
        // In reflection contexts, for attributes that can have "unset" as a value that means null,
        // we have to check this state with an additional property. E.g. ExecuteAttribute.ValidateAttributes.
        // In symbol contexts, the absence of syntax that assigns a value to the property implies this "unset" state.
        if (hasValueExpression is not null)
        {
            // `hasValue` will always be non-null in reflection contexts,
            // and always null in symbol contexts where the HasValue property won't ever be set.
            var hasValue = GetValue<bool>(hasValueExpression.GetExpressedProperty().Name);
            if (hasValue == false)
            {
                return null;
            }
        }


        return GetValue<T>(propertyExpression.GetExpressedProperty().Name);
    }

    public abstract object? GetValue(string valueName);
}
