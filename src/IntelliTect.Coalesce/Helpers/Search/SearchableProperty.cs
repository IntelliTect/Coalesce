using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers.Search
{
    public abstract class SearchableProperty
    {
        public SearchableProperty(PropertyViewModel prop)
        {
            Property = prop;
        }

        public PropertyViewModel Property { get; protected set; }

        protected virtual string PropertyNamePath => Property.Name;

        public abstract IEnumerable<(PropertyViewModel property, string statement)> GetLinqDynamicSearchStatements(ClaimsPrincipal user, string propertyParent, string rawSearchTerm);
    }
}
