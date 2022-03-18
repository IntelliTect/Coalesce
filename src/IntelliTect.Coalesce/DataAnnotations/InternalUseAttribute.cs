using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.AttributeTargets;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Types and members marked [InternalUse] are not exposed by Coalesce's generated APIs.
    /// </summary>
    [System.AttributeUsage(Property | Method | Class | Struct)]
    public class InternalUseAttribute : Attribute
    {
    }
}
