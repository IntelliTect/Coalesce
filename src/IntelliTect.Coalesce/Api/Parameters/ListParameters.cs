using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api
{
    public class ListParameters : FilterParameters, IListParameters
    {
        /// <inheritdoc />
        public int? Page { get; set; }

        /// <inheritdoc />
        public int? PageSize { get; set; }

        /// <inheritdoc />
        public string OrderBy { get; set; }

        /// <inheritdoc />
        public string OrderByDescending { get; set; }
        
        /// <inheritdoc />
        public string Fields { get; set; }

        private IReadOnlyCollection<string> _fieldList;
        /// <inheritdoc cref="IListParameters.Fields" />
        public IReadOnlyCollection<string> FieldList
        {
            get
            {
                if (_fieldList != null)
                {
                    return _fieldList;
                }
                if (Fields == null)
                {
                    return _fieldList = new List<string>();
                }
                return _fieldList = Fields.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        
        /// <inheritdoc />
        IReadOnlyCollection<string> IListParameters.Fields => FieldList;
        
        /// <inheritdoc cref="IListParameters.OrderByList" />
        public Dictionary<string, SortDirection> OrderByList
        {
            get
            {
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
                return result;
            }
        }
        
        /// <inheritdoc />
        IReadOnlyDictionary<string, SortDirection> IListParameters.OrderByList => OrderByList;
    }
}
