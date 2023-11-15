using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.Helpers.Search
{
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

            var accessor = Expression.Property(propertyParent, Property.Name);

            return Children
                .SelectMany(c => {
                    var anyParam = Expression.Parameter(Property.PureType.TypeInfo);

                    return c.GetLinqSearchStatements(context, anyParam, rawSearchTerm).Select(t =>
                    {
                        var expr = accessor.Call(
                            EnumerableAnyWithPredicate.MakeGenericMethod(Property.PureType.TypeInfo),
                            Expression.Lambda(
                                t.statement,
                                anyParam
                            )
                        );

                        // This will get optimized away when translating to SQL,
                        // but guarding against null is necessary to perform our
                        // search tests in memory.
                        expr = Expression.AndAlso(
                            Expression.NotEqual(accessor, Expression.Constant(null)),
                            expr
                        );

                        return (t.property, expr);
                    });
                });
        }

        private static readonly MethodInfo EnumerableAnyWithPredicate
            = typeof(Enumerable).GetMethods().Single(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2);
    }
}
