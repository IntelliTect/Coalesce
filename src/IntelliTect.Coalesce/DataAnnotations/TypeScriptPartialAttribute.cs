using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class TypeScriptPartialAttribute : Attribute
    {
        public string BaseClassName { get; set; }
    }
}
