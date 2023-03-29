using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public ExecuteAttribute()
        {
        }

        public ExecuteAttribute(SecurityPermissionLevels permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public ExecuteAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }

        /// <summary>
        /// If true, admin pages will clear the parameter inputs when a successful invocation is performed.
        /// </summary>
        public bool AutoClear { get; set; }

        /// <summary>
        /// If true, validation of <see cref="ValidationAttribute"/> on parameters will be performed by the server.
        /// This setting defaults to the value of <see cref="CoalesceOptions.ValidateAttributesForMethods"/>.
        /// </summary>
        public bool? ValidateAttributes { get; set; }
    }
}
