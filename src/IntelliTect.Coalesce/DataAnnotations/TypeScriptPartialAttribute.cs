using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    [Obsolete("This attribute only affects generated Knockout.js code. The Knockout stack for Coalesce is obsolete.")]
    public class TypeScriptPartialAttribute : Attribute
    {
        public string? BaseClassName { get; set; }
    }
}
