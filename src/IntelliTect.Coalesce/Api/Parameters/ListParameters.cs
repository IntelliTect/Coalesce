using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace IntelliTect.Coalesce.Api;

public class ListParameters : FilterParameters, IListParameters
{
    /// <inheritdoc />
    public int? Page { get; set; }

    /// <inheritdoc />
    public int? PageSize { get; set; }

    /// <inheritdoc />
    public bool? NoCount { get; set; }

    /// <inheritdoc />
    public string? OrderBy { get; set; }

    /// <inheritdoc />
    public string? OrderByDescending { get; set; }
    
    /// <inheritdoc />
    public string? Fields { get; set; }

    private string? _cachedFields;
    private IReadOnlyCollection<string>? _fieldList;
    /// <inheritdoc cref="IListParameters.Fields" />
    public IReadOnlyCollection<string> FieldList
    {
        get
        {
            if (_fieldList != null && Fields == _cachedFields)
            {
                return _fieldList;
            }

            // Threadsafety - null _fieldList so an intermediate state isn't incorrectly served.
            _fieldList = null;
            _cachedFields = Fields;
            if (Fields == null)
            {
                return _fieldList = new List<string>()
                    .AsReadOnly();
            }

            return _fieldList = Fields
                .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .ToList()
                .AsReadOnly();
        }
    }
    
    /// <inheritdoc />
    IReadOnlyCollection<string> IListParameters.Fields => FieldList;

    /// <inheritdoc cref="IListParameters.OrderByList" />
    private string? _cachedOrderBy;
    private string? _cachedOrderByDescending;
    private IReadOnlyDictionary<string, SortDirection>? _orderByList;
    public IReadOnlyDictionary<string, SortDirection> OrderByList
    {
        get
        {
            if (_orderByList != null && OrderBy == _cachedOrderBy && OrderByDescending == _cachedOrderByDescending)
            {
                return _orderByList;
            }

            var result = new Dictionary<string, SortDirection>();
            // Add order by
            if (OrderBy != null)
            {
                foreach (var orderBy in OrderBy.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = orderBy.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                    {
                        // Ignore if all entries were empty.
                    }
                    else if (parts.Length == 2 && parts[1].StartsWith("D", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(parts[0], SortDirection.Desc);
                    }
                    else
                    {
                        result.Add(parts[0], SortDirection.Asc);
                    }
                }
            }
            // Add order by descending
            if (OrderByDescending != null)
            {
                foreach (var orderBy in OrderByDescending.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = orderBy.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                    {
                        // Ignore if all entries were empty.
                    }
                    else
                    {
                        result.Add(parts[0], SortDirection.Desc);
                    }
                }
            }


            // Threadsafety - null _orderByList so an intermediate state isn't incorrectly served.
            _orderByList = null;
            _cachedOrderBy = OrderBy;
            _cachedOrderByDescending = OrderByDescending;
            return _orderByList = new ReadOnlyDictionary<string, SortDirection>(result);
        }
    }
    
    /// <inheritdoc />
    IReadOnlyDictionary<string, SortDirection> IListParameters.OrderByList => OrderByList;
}
