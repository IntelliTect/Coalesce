using System;
using System.Collections.Generic;
using System.Text;

// Explicitly in IntelliTect.Coalesce to simplify typical using statements 
namespace IntelliTect.Coalesce
{

    public interface IFilterParameters : IDataSourceParameters
    {
        /// <summary>
        /// A free-form search string specified by the requester.
        /// </summary>
        string Search { get; }

        /// <summary>
        /// A mapping of values, keyed by field name, on which to filter.
        /// It is the responsibility of the consumer to decide how to interpret these values.
        /// </summary>
        IDictionary<string, string> Filter { get; }
    }
}
