using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Properties marked ReadOnlyApi are not saved.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ReadOnlyApiAttribute : System.Attribute
    {
    }
}
