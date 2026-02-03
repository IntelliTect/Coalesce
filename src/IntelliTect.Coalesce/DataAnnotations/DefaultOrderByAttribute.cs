using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Allows specifying the default sort order for returns lists of this object type.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property)]
public class DefaultOrderByAttribute : System.Attribute
{
    public enum OrderByDirections
    {
        Ascending = 0,
        Descending = 1
    }

    public OrderByDirections OrderByDirection { get; set; }
    public int FieldOrder { get; set; }

    /// <summary>
    /// When using the DefaultOrderByAttribute on an object property, specifies the field on the object to use for sorting.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// When true, suppresses using this property as a fallback ordering.
    /// Useful for preventing automatic ordering by Name or primary key properties.
    /// </summary>
    public bool Suppress { get; set; }

    public DefaultOrderByAttribute(int fieldOrder = 0, OrderByDirections orderByDirection = OrderByDirections.Ascending)
    {
        this.OrderByDirection = orderByDirection;
        this.FieldOrder = fieldOrder;
    }
}

public record OrderByInformation
{
    /// <summary>
    /// The property to order by. If this contains multiple properties,
    /// it represents chained ancestor properties of the desired property.
    /// </summary>
    public IReadOnlyList<PropertyViewModel> Properties { get; init; } = Array.Empty<PropertyViewModel>();

    public DefaultOrderByAttribute.OrderByDirections OrderByDirection { get; init; }

    public SortDirection SortDirection
    {
        get => OrderByDirection == DefaultOrderByAttribute.OrderByDirections.Ascending
            ? SortDirection.Asc
            : SortDirection.Desc;
        init => OrderByDirection = value == SortDirection.Asc
            ? DefaultOrderByAttribute.OrderByDirections.Ascending
            : DefaultOrderByAttribute.OrderByDirections.Descending;
    }

    public int FieldOrder { get; init; }

    /// <summary>
    /// The type that will be compared to perform the sort.
    /// </summary>
    public Type TypeInfo => Properties.Last().Type.TypeInfo;

    public string OrderExpression(string prependText = "")
    {
        string text = Properties.Count > 1 ? "(" : "";
        string propAccessor = prependText;
        propAccessor += (propAccessor == "" ? "" : ".") + Properties[0].Name;
        foreach (var prop in Properties.Skip(1))
        {
            text += $"{propAccessor} == null ? {prop.Type.CsDefaultValue} : ";
            propAccessor += (propAccessor == "" ? "" : ".") + prop.Name;
        }

        text += propAccessor + (Properties.Count > 1 ? ")" : "");

        return text;
    }

    /// <summary>
    /// Produces a lambda representing this ordering, suitable for passing 
    /// as an argument to the result of <see cref="OrderByMethod{T}(bool)"/>.
    /// </summary>
    public Expression LambdaExpression<T>()
    {
        var param = Expression.Parameter(typeof(T));
        return Expression.Lambda(OrderExpression(param), param);
    }

    /// <summary>
    /// Produces the property expression for the property access chain
    /// represented by <see cref="Properties"/> that will yield the value being sorted.
    /// </summary>
    public Expression OrderExpression(Expression parent)
    {
        return Recurse(parent, Properties);

        Expression Recurse(Expression parent, IEnumerable<PropertyViewModel> props)
        {
            var prop = props.First();
            var propAccess = Expression.Property(parent, prop.Name);

            var next = props.Count() == 1
                ? propAccess
                : Recurse(propAccess, props.Skip(1));

            if (parent.NodeType == ExpressionType.MemberAccess)
            {
                // We're chaining off an object. Make sure the object isnt null,
                // then recursively build the deeper accessor:
                return Expression.Condition(
                    Expression.Equal(parent, Expression.Constant(null)),
                    Expression.Default(TypeInfo),
                    next
                );
            }
            else
            {
                // Not chaining off an object. Probably off a ParameterExpression
                // (i.e. the `x` in `.OrderBy(x => x.Foo)`.
                return next;
            }
        }
    }

    public MethodInfo OrderByMethod(bool isPrimary)
    {
        return (isPrimary, SortDirection) switch
        {
            (true, SortDirection.Asc) => QueryableMethods.OrderBy,
            (true, SortDirection.Desc) => QueryableMethods.OrderByDescending,
            (false, SortDirection.Asc) => QueryableMethods.ThenBy,
            (false, SortDirection.Desc) => QueryableMethods.ThenByDescending,
            _ => throw new ArgumentException("Unknown SortDirection")
        };
    }

    public MethodInfo OrderByMethod<T>(bool isPrimary)
    {
        return OrderByMethod(isPrimary).MakeGenericMethod(typeof(T), TypeInfo);
    }

    public override string ToString()
        => string.Join('.', Properties.Select(p => p.Name)) + " " + SortDirection;
}
