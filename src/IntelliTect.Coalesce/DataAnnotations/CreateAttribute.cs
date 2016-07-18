using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Should users be allowed to create via the API/button.
    /// </summary>    
    [System.AttributeUsage(AttributeTargets.Class)]
    public class CreateAttribute: System.Attribute
    {
        public bool Allow { get; set; } = true;
        // TODO: Add security to create
        //public string Roles { get; set; }
        //public bool AllowAnonymous { get; set; }

    }
}
