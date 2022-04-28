using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    /// <summary>
    /// Class that contains security information for a class or property based on the Read and Edit attributes
    /// </summary>
    public class PropertySecurityInfo
    {
        public PropertySecurityInfo(PropertyViewModel prop)
        {
            var read = prop.GetSecurityPermission<ReadAttribute>();
            var edit = prop.GetSecurityPermission<EditAttribute>();

            if (
                read.NoAccess ||
                !prop.IsClientProperty
            )
            {
                Read = new PropertySecurityPermission(false, null, read.Name);
            }
            else
            {
                Read = new PropertySecurityPermission(true, read.Roles, read.Name);
            }

            if (
                edit.NoAccess ||
                !prop.IsClientProperty ||
                !prop.HasPublicSetter ||

                // Properties with a [Read] attribute but no [Edit] attribute
                // are read-only. This is for multiple reasons:
                // 1) The syntax feels natural - seeing a property with [Read] and no other attributes
                //    just /feels/ like it should be read-only.
                // 2) It avoids accidents by omission - applying specific permissions for reading
                //    but not applying permissions for editing is probably an accident that would otherwise introduce a security hole.
                (read.HasAttribute && !edit.HasAttribute) ||

                prop.HasAttribute<ReadOnlyAttribute>() ||

#pragma warning disable CS0618 // Type or member is obsolete
                prop.HasAttribute<ReadOnlyApiAttribute>() ||
#pragma warning restore CS0618 // Type or member is obsolete

                // Non-value properties arent writable unless they're owned by an external type,
                // or the type of the property is an external type.
                (prop.PureType.IsPOCO && (prop.Parent.IsDbMappedType || prop.Object!.IsDbMappedType))
            )
            {
                Edit = new PropertySecurityPermission(false, null, edit.Name);
            }
            else
            {
                Edit = new PropertySecurityPermission(true, edit.Roles, edit.Name);
            }
        }

        public PropertySecurityPermission Read { get; }
        public PropertySecurityPermission Edit { get; }

        /// <summary>
        /// If true, the user can read the field.
        /// </summary>
        public bool IsReadable(ClaimsPrincipal? user) => Read.IsAllowed(user);

        /// <summary>
        /// If true, the user can edit the field.
        /// </summary>
        public bool IsEditable(ClaimsPrincipal? user) => Edit.IsAllowed(user);


        public override string ToString()
        {
            return $"Read:{Read}  Edit:{Edit}";
        }

    }
}
