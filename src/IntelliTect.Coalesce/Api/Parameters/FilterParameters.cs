using System.Collections.Generic;

namespace IntelliTect.Coalesce.Api
{
    public class FilterParameters : DataSourceParameters, IFilterParameters
    {
        /// <summary>
        /// Text to search fields for.
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// List of filters added from the controller based on property=value on the URL.
        /// </summary>
        public Dictionary<string, string> Filter { get; } = new Dictionary<string, string>();

        IDictionary<string, string> IFilterParameters.Filter => Filter;
    }
}
