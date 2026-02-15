using System.Collections.Generic;

// Explicitly in IntelliTect.Coalesce to simplify typical using statements 
namespace IntelliTect.Coalesce;

public interface IListParameters : IFilterParameters
{
    /// <summary>
    /// The page of data being requested. 
    /// </summary>
    int? Page { get; }

    /// <summary>
    /// The size of pages requested.
    /// If this value is unacceptable, it may be constrained to some acceptable value,
    /// or alternatively, an error result may be served.
    /// </summary>
    int? PageSize { get; }

    /// <summary>
    /// If true, a total count of items will not be determined. `pageCount` and `totalCount` will be returned as -1.
    /// </summary>
    bool? NoCount { get; }

    /// <summary>
    /// A comma-delimited list of field names to sort by.
    /// Each field name may be followed by "ASC" or "DESC" (case insensitive), specifying direction.
    /// </summary>
    string? OrderBy { get; }
    
    /// <summary>
    /// A comma-delimited list of field names to sort by in DESCENDING order.
    /// </summary>
    string? OrderByDescending { get; }
    
    /// <summary>
    /// An interpreted list of both OrderBy and OrderByDescending.
    /// Keys are field names, and values are directions.
    /// If both OrderBy and OrderByDescending were populated, the values in OrderBy supercede those in OrderByDescending.
    /// </summary>
    IReadOnlyDictionary<string, SortDirection> OrderByList { get; }

    /// <summary>
    /// A list of field names being requested.
    /// If this collection is non-null and contains any values,
    /// the response should only include those field names which were specified.
    /// </summary>
    IReadOnlyCollection<string> Fields { get; }
}
