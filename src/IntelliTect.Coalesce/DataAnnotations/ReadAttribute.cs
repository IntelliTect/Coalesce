using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// <para>
    /// The Class or Property is read only for the users and groups and not accessible to others.
    /// If no roles are specified, the target is readable by anyone.
    /// </para>
    /// <para>If specified on a property with no <see cref="EditAttribute"/>, the property is read-only.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadAttribute : SecurityAttribute
    {
        public ReadAttribute()
        {
        }

        public ReadAttribute(SecurityPermissionLevels permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public ReadAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }

        /// <summary>
        /// <para>
        /// If true, the class or navigation property targeted by the attribute will not be loaded by Coalesce's 
        /// <see cref="QueryableExtensions.IncludeChildren"/> method, 
        /// nor by the <a href="https://intellitect.github.io/Coalesce/modeling/model-components/data-sources.html#default-loading-behavior">Default Loading Behavior</a>, 
        /// which normally automatically includes the first level of navigation properties of any entity returned from a Coalesce-generated /get, /list, or /save API endpoint.
        /// </para>
        /// <para>
        /// If true, you must always Include the desired navigation properties in a custom data source, or load entites directly from the type's /get or /list endpoints.
        /// </para>
        /// <para>
        /// Use this to prevent accidental unrestricted, unfiltered loading of types that hold sensitive information.</para>
        /// </summary>
        public bool NoAutoInclude { get; set; }
    }
}
