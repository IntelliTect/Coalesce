using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Properties marked internal are not added to the API.
    /// Methods do not show up in the API.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class InternalUseAttribute : System.Attribute
    {
    }
}
