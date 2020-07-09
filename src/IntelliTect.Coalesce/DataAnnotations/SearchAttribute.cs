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
            /// <summary>
            /// Search term will be checked for at the beginning of the field's value in a case insensitive manner.
            /// </summary>
            BeginsWith = 1,

            /// <summary>
            /// Search term will be checked for anywhere inside the field's value in a case insensitive manner.
            /// </summary>
            Contains = 2,

            /// <summary>
            /// Search term must match the field exactly in a case insensitive manner.
            /// </summary>
            Equals = 3,

            /// <summary>
            /// <para>
            /// Search term must match exactly, using the natural casing handling of the evaluation environment.
            /// </para>
            /// 
            /// <para>
            /// Default database collation will be used if evaluated in SQL,
            /// and exact casing will be used if evaluated in memory.
            /// </para>
            /// 
            /// <para>
            /// This allows index seeks to be used instead of index scans,
            /// providing extra high performance searches against indexed columns.
            /// </para>
            /// </summary>
            EqualsNatural = 4,
        };

        /// <summary>
        /// If set to true (the default), each word in the search terms will be searched for in each searchable field independently.
        /// </summary>
        public bool IsSplitOnSpaces { get; set; } = true;

        /// <summary>
        /// <para>
        /// Specifies how string columns are searched. See individual enum members for details.
        /// </para>
        /// <para>
        /// Has no effect on non-string values. Numbers, GUIDs, and enums are always searched with exact values. 
        /// Dates are searched with lower and upper bounds if the user input could be parsed as a partial or complete date.
        /// </para>
        /// </summary>
        public SearchMethods SearchMethod { get; set; } = SearchMethods.BeginsWith;

        /// <summary>
        /// A comma-delimited list of model class names that, if set,
        /// will restrict the targeted property from being searched unless the
        /// root object of the API call was one of the specified class names.
        /// </summary>
        public string? RootWhitelist { get; set; }
        
        /// <summary>
        /// A comma-delimited list of model class names that, if set,
        /// will restrict the targeted property from being searched if
        /// the root object of the API call was one of the specified class names.
        /// </summary>
        public string? RootBlacklist { get; set; }
    }
}
