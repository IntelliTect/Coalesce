using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using IntelliTect.Coalesce.Utilities;

namespace IntelliTect.Coalesce.Helpers.Search
{
    public class SearchableValueProperty : SearchableProperty
    {
        public SearchableValueProperty(PropertyViewModel prop) : base(prop)
        {
        }

        public override IEnumerable<(PropertyViewModel property, string statement)> GetLinqDynamicSearchStatements(ClaimsPrincipal user, string propertyParent, string rawSearchTerm)
        {
            if (!Property.SecurityInfo.IsReadable(user))
            {
                yield break;
            }

            var propertyAccessor = propertyParent == null
                ? Property.Name
                : $"{propertyParent}.{Property.Name}";

            if (Property.Type.IsDate)
            {

            }
            else if (Property.Type.IsString)
            {
                var term = rawSearchTerm.EscapeStringLiteralForLinqDynamic();
                yield return (Property, $"({propertyAccessor} != null && {propertyAccessor}.{string.Format(Property.SearchMethodCall, term)})");
            }
            else
            {

            }
        }
    }
}
