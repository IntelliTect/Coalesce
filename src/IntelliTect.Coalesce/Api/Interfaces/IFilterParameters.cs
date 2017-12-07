using System;
using System.Collections.Generic;
using System.Text;

// Explicitly in IntelliTect.Coalesce to simplify typical using statements 
namespace IntelliTect.Coalesce
{

    public interface IFilterParameters : IDataSourceParameters
    {
        string Where { get; }

        string Search { get; }

        IDictionary<string, string> Filter { get; }
    }
}
