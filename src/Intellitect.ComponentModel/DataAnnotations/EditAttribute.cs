using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.DataAnnotations
{
    /// <summary>
    /// The Class or Property is read/write for the users and groups and not accessible to others.
    /// </summary>    
    [System.AttributeUsage(System.AttributeTargets.Property | AttributeTargets.Class)]
    public class EditAttribute: System.Attribute
    {
        public string Roles { get; set; }
        public bool AllowAnonymous { get; set; }
        /// <summary>
        /// If true, editing is possible. If false, no one can edit.
        /// </summary>
        public bool Allow { get; set; } = true;

    }
}
