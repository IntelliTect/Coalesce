using Microsoft.AspNetCore.Http;
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
        
        /// <inheritdoc cref="IListParameters.Fields" />
        public List<string> FieldList
        {
            get
            {
                if (Fields == null) return new List<string>();
                return Fields.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        
        /// <inheritdoc />
        ICollection<string> IListParameters.Fields => FieldList;
        
        /// <inheritdoc cref="IListParameters.OrderByList" />
        public Dictionary<string, string> OrderByList
        {
            get
            {
                var result = new Dictionary<string, string>();
                // Add order by
                if (OrderBy != null)
                {
                    foreach (var orderBy in OrderBy.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = orderBy.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2 && parts[1].ToUpper().StartsWith("D"))
                        {
                            result.Add(parts[0], "Desc");
                        }
                        else {
                            result.Add(parts[0], "Asc");
                        }
                    }
                }
                // Add order by descending
                if (OrderByDescending != null)
                {
                    foreach (var orderBy in OrderByDescending.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        result.Add(orderBy, "Desc");
                    }
                }
                return result;
            }
        }
        
        /// <inheritdoc />
        IDictionary<string, string> IListParameters.OrderByList => OrderByList;
    }
}
