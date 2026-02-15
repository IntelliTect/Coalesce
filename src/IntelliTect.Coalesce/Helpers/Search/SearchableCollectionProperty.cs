using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IntelliTect.Coalesce.Helpers.Search;

internal class SearchableCollectionProperty : SearchableProperty
{
    internal SearchableCollectionProperty(PropertyViewModel propertyViewModel, IEnumerable<SearchableProperty> children) : base(propertyViewModel)
    {
        Children = children.ToList();
    }

    internal ICollection<SearchableProperty> Children { get; }

    public override IEnumerable<(PropertyViewModel property, Expression statement)> GetLinqSearchStatements(
        CrudContext context, Expression propertyParent, string rawSearchTerm)
    {
        if (!Property.SecurityInfo.IsFilterAllowed(context))
        {
            return Enumerable.Empty<(PropertyViewModel, Expression)>();
        }

        // collection: `parent.Children`
        var collection = Expression.Property(propertyParent, Property.Name);
        var anyParam = Expression.Parameter(Property.PureType.TypeInfo);

        var childExprs = Children
            .SelectMany(c => c
                .GetLinqSearchStatements(context, anyParam, rawSearchTerm)
                .Select(x => x.statement)
            ).OrAny();

        return [(Property, Expression.AndAlso(
            
            // This NotEqual will get optimized away when translating to SQL,
            // but guarding against null is necessary to perform our
            // search tests in memory.
            Expression.NotEqual(collection, Expression.Constant(null)),
            collection.Call(
                EnumerableAnyWithPredicate.MakeGenericMethod(Property.PureType.TypeInfo),
                Expression.Lambda(
                    childExprs,
                    anyParam
                )
            )
        ))];
    }

    private static readonly MethodInfo EnumerableAnyWithPredicate
        = typeof(Enumerable).GetMethods().Single(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2);
}
