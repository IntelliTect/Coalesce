using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SelectFilterAttribute : Attribute
    {
        /// <summary>
        /// The name of the property on the foreign object to filter against.
        /// </summary>
        public string? ForeignPropertyName { get; set; }

        /// <summary>
        /// The name of another property belonging to the class in which this attribute is used.
        /// The results of select lists will be filtered to match this value.
        /// 
        /// Defaults to the value of ForeignPropertyName unless LocalPropertyValue is set.
        /// </summary>
        public string? LocalPropertyName { get; set; }

        /// <summary>
        /// If specified, the LocalPropertyName will be resolved from the property by this name that resides on the local object.
        /// 
        /// This allows for querying against properties that are one level away from the current object.
        /// </summary>
        public string? LocalPropertyObjectName { get; set; }

        /// <summary>
        /// A constant value that the foreign property will be filtered against.
        /// 
        /// If this is set, LocalPropertyName will be ignored.
        /// </summary>
        public string? StaticPropertyValue { get; set; }
    }
}
