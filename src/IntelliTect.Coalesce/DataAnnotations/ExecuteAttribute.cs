using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// The method can be excuted by the specified role.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExecuteAttribute : SecurityAttribute
    {

    }
}
