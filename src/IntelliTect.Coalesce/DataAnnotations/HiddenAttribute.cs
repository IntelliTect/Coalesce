using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Allows this property to be hidden on the list or editor or both.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class HiddenAttribute : System.Attribute
    {
        public enum Areas
        {
            All = 0,
            List = 1,
            Edit = 2
        }
        public Areas Area { get; set; }

        public HiddenAttribute(Areas area = Areas.All)
        {
            this.Area = area;
        }
    }
}
