using System.Collections.Generic;

namespace IntelliTect.Coalesce.Api
{
    public class FilterParameters : DataSourceParameters, IFilterParameters
    {
        /// <inheritdoc />
        public string Search { get; set; }

        /// <inheritdoc cref="IFilterParameters.Filter" />
        public Dictionary<string, string> Filter { get; } = new Dictionary<string, string>();
        
        /// <inheritdoc />
        IDictionary<string, string> IFilterParameters.Filter => Filter;
    }
}
