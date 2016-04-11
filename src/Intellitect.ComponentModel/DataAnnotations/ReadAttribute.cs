using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.DataAnnotations
{
    /// <summary>
    /// The Class or Property is read only for the users and groups and not accessible to others.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadAttribute: System.Attribute
    {
        public bool AllowAnonymous { get; set; }
        public string Roles { get; set; }
    }
}
