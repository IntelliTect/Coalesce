using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Should users be allowed to delete via the API/button.
    /// </summary>    
    [System.AttributeUsage(AttributeTargets.Class)]
    public class DeleteAttribute: System.Attribute
    {
        public bool Allow { get; set; } = true;
        // Add security to delete
        public string Roles { get; set; }
        public bool AllowAnonymous { get; set; }

    }
}
