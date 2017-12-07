using System.Collections.Generic;

// Explicitly in IntelliTect.Coalesce to simplify typical using statements 
namespace IntelliTect.Coalesce
{
    public interface IListParameters : IFilterParameters
    {
        int? Page { get; }

        int? PageSize { get; }

        string OrderBy { get; }

        string OrderByDescending { get; }

        // TODO: type this better than string,string.
        // The values are [columnName] = "Asc|Desc"
        IDictionary<string, string> OrderByList { get; }

        ICollection<string> Fields { get; }
    }
}
