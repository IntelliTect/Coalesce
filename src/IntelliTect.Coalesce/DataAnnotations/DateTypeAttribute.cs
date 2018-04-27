using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Allows specifying the type of date to contain. 
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class DateTypeAttribute : System.Attribute
    {
        public enum DateTypes
        {
            DateTime = 0,
            DateOnly = 1,
        }

        public DateTypes DateType { get; set; }

        public DateTypeAttribute(DateTypes dateType = DateTypes.DateTime)
        {
            this.DateType = dateType;
        }
    }
}
