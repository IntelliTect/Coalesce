using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;

namespace IntelliTect.Coalesce.Helpers.Search
{
    internal class SearchableCollectionProperty : SearchableProperty
    {
        internal SearchableCollectionProperty(PropertyViewModel propertyViewModel, IEnumerable<SearchableProperty> children) : base(propertyViewModel)
        {
            Children = children.ToList();
        }

        internal ICollection<SearchableProperty> Children { get; }

        public override IEnumerable<(PropertyViewModel property, string statement)> GetLinqDynamicSearchStatements(
            ClaimsPrincipal user, string propertyParent, string rawSearchTerm)
        {
            if (!Property.SecurityInfo.IsReadable(user))
            {
                return Enumerable.Empty<(PropertyViewModel, string)>();
            }

            var accessor = propertyParent == null
                ? Property.Name
                : $"{propertyParent}.{Property.Name}";

            return Children
                .SelectMany(c => {
                    return c.GetLinqDynamicSearchStatements(user, null, rawSearchTerm).Select(t =>
                    {
                        return (t.property, $"{accessor}.Any({t.statement})");
                    });
                });
        }
    }
}
