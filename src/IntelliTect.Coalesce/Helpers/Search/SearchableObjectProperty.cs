using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers.Search
{
    public class SearchableObjectProperty : SearchableProperty
    {
        public SearchableObjectProperty(PropertyViewModel propertyViewModel, IEnumerable<SearchableProperty> children) : base(propertyViewModel)
        {
            Children = children.ToList();
        }

        internal ICollection<SearchableProperty> Children { get; } = new List<SearchableProperty>();

        public override IEnumerable<(PropertyViewModel property, Expression statement)> GetLinqSearchStatements(
            CrudContext context, Expression propertyParent, string rawSearchTerm)
        {
            if (!Property.SecurityInfo.IsFilterAllowed(context))
            {
                return Enumerable.Empty<(PropertyViewModel, Expression)>();
            }

            var parent = Expression.Property(propertyParent, Property.Name);

            return Children
                .SelectMany(c => c.GetLinqSearchStatements(context, parent, rawSearchTerm));
        }
    }
}
