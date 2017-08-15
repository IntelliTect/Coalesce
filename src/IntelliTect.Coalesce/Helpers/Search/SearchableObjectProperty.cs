using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override IEnumerable<(PropertyViewModel, string)> GetLinqDynamicSearchStatements(ClaimsPrincipal user, string propertyParent, string rawSearchTerm)
        {
            var parent = propertyParent == null
                ? Property.Name
                : $"{propertyParent}.{Property.Name}";

            return Children
                .SelectMany(c => c.GetLinqDynamicSearchStatements(user, parent, rawSearchTerm));


        }
    }
}
