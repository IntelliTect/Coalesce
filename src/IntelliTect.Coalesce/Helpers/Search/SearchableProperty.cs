using IntelliTect.Coalesce.TypeDefinition;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.Helpers.Search;

public abstract class SearchableProperty
{
    public SearchableProperty(PropertyViewModel prop)
    {
        Property = prop;
    }

    public PropertyViewModel Property { get; protected set; }

    protected virtual string PropertyNamePath => Property.Name;

    public abstract IEnumerable<(PropertyViewModel property, Expression statement)> GetLinqSearchStatements(
        CrudContext context, Expression propertyParent, string rawSearchTerm);
}
