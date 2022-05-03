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
                Read = new PropertySecurityPermission(prop, false, null, read.Name);
            }
            else
            {

                Read = new PropertySecurityPermission(prop, true, read.Roles, read.Name, IsReadUnused);
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
                Edit = new PropertySecurityPermission(prop, false, null, edit.Name);
            }
            else
            {
                Edit = new PropertySecurityPermission(prop, true, edit.Roles, edit.Name, IsEditUnused);
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

        private static bool? IsReadUnused(IValueViewModel value, HashSet<PropertyViewModel> visited)
        {
            // The client only needs to Edit/Write to parameters, so read IS unused.
            if (value is ParameterViewModel) return true;
            // The client needs to Read from method returns, so read is not unused.
            if (value is MethodReturnViewModel) return false;

            if (value is PropertyViewModel p)
            {
                if (!visited.Add(p)) return null;
                if (visited.Count > 1 && !p.SecurityInfo.Read.IsAllowed()) return true;

                if (p.EffectiveParent is { IsDbMappedType: true } or { IsStandaloneEntity: true })
                {
                    // Reads will be done by generated APIs
                    return p.EffectiveParent.SecurityInfo.Read.NoAccess;
                }

                foreach (var usage in p.EffectiveParent.Usages)
                {
                    if (IsReadUnused(usage, visited) == false) return false;
                }
            }
            return true;
        }

        private static bool? IsEditUnused(IValueViewModel value, HashSet<PropertyViewModel> visited)
        {
            // The client needs to Edit/Write to parameters, so edit is NOT unused.
            if (value is ParameterViewModel) return false;
            // The client only needs to Read from method returns, so edit IS unused.
            if (value is MethodReturnViewModel) return true;

            if (value is PropertyViewModel p)
            {
                if (!visited.Add(p)) return null;
                if (visited.Count > 1 && !p.SecurityInfo.Edit.IsAllowed()) return true;

                if (p.EffectiveParent is { IsDbMappedType: true } or { IsStandaloneEntity: true })
                {
                    // Edits will be done by generated APIs
                    return p.EffectiveParent.SecurityInfo.Save.NoAccess;
                }

                foreach (var usage in p.EffectiveParent.Usages)
                {
                    if (IsEditUnused(usage, visited) == false) return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return $"Read:{Read}  Edit:{Edit}";
        }

    }
}
