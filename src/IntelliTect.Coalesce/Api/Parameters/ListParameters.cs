using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Api
{
    public class ListParameters : FilterParameters, IListParameters
    {
        public string OrderBy { get; set; }
        public string OrderByDescending { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        /// <summary>
        /// CSV list of fields to return
        /// </summary>
        public string Fields { get; set; }

        /// <summary>
        /// Calculated list from Fields
        /// </summary>
        public List<string> FieldList
        {
            get
            {
                if (Fields == null) return new List<string>();
                return Fields.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        ICollection<string> IListParameters.Fields => FieldList;

        public ListParameters() { }

        public ListParameters(string fields = null,
            string includes = null, string orderBy = null, string orderByDescending = null,
            int? page = null, int? pageSize = null, string where = null,
            string dataSource = null, string search = null)
        {
            Fields = fields;
            Includes = includes;
            OrderBy = orderBy;
            OrderByDescending = orderByDescending;
            Page = page;
            PageSize = pageSize;
            Where = where;
            DataSource = dataSource;
            Search = search;
        }

        /// <summary>
        /// List of OrderBy clauses keyed by column and with a value of Asc or Desc.
        /// </summary>
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

        IDictionary<string, string> IListParameters.OrderByList => OrderByList;

        /// <summary>
        /// Adds a name value condition to the Filters list.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void AddFilter(string propertyName, string propertyValue)
        {
            if (propertyValue != null)
            {
                Filter[propertyName] = propertyValue;
            }
        }
    }
}
