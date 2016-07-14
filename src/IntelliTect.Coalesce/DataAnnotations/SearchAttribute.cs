using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{

    /// <summary>
    /// All fields marked with this field will be searched when using the list search feature.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SearchAttribute : System.Attribute
    {
        public enum SearchMethods
        {
            BeginsWith = 1,
            Contains = 2
        };

        public bool IsSplitOnSpaces { get; set; } = true;
        public SearchMethods SearchMethod { get; set; } = SearchMethods.BeginsWith;

    }
}
