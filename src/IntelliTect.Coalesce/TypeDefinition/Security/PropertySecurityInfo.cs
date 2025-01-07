using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IntelliTect.Coalesce.TypeDefinition
{
    /// <summary>
    /// Class that contains security information for a class or property based on the Read and Edit attributes
    /// </summary>
    public class PropertySecurityInfo
    {
        private PropertyViewModel Prop { get; }

        public PropertySecurityInfo(PropertyViewModel prop)
        {
            Prop = prop;

            var read = prop.GetSecurityPermission<ReadAttribute>();
            var edit = prop.GetSecurityPermission<EditAttribute>();

            string? readDenyReason =
                read.NoAccess ? "Property annotated with [Read(SecurityPermissionLevels.DenyAll)]" :
                !prop.HasGetter ? "Property has no get accessor" :
                prop.IsInternalUse ? "Property annotated with [InternalUse]" :
                !prop.IsClientProperty ? "Other" :
                null;

            if (readDenyReason != null)
            {
                Read = new PropertySecurityPermission(prop, read.Name, readDenyReason);
            }
            else
            {
                Read = new PropertySecurityPermission(prop, read.Name, read.RoleLists, IsReadUnused);
            }


            string? editDenyReason =
                edit.NoAccess ? "Property annotated with [Edit(SecurityPermissionLevels.DenyAll)]" :
                !prop.HasPublicSetter ? "Property has no public set accessor" :
                prop.IsInternalUse ? "Property annotated with [InternalUse]" :

                // Properties with a [Read] attribute but no [Edit] attribute
                // are read-only. This is for multiple reasons:
                // 1) The syntax feels natural - seeing a property with [Read] and no other attributes
                //    just /feels/ like it should be read-only.
                // 2) It avoids accidents by omission - applying specific permissions for reading
                //    but not applying permissions for editing is probably an accident that would otherwise introduce a security hole.
                (read.HasAttribute && !edit.HasAttribute) ? "Property has [Read] attribute, but doesn't have an [Edit] attribute" :

                prop.HasAttribute<ReadOnlyAttribute>() ? "Property annotated with [ReadOnly]" :

#pragma warning disable CS0618 // Type or member is obsolete
                prop.HasAttribute<ReadOnlyApiAttribute>() ? "Property annotated with [ReadOnlyApi]" :
#pragma warning restore CS0618 // Type or member is obsolete

                // Non-scalar properties arent writable unless they're owned by an external type,
                // or the type of the property is an external type.
                (prop.PureType.IsPOCO && prop.Parent.IsDbMappedType) ? $"Property is non-scalar and parent type {prop.Parent.Name} is a DB mapped type." :
                (prop.PureType.IsPOCO && prop.Object!.IsDbMappedType) ? $"Property is non-scalar and property type {prop.Object.Name} is a DB mapped type." :

                !prop.IsClientProperty ? "Other" :
                null;

            if (editDenyReason != null)
            {
                // Property is readonly.
                Init = new PropertySecurityPermission(prop, nameof(Init), editDenyReason);
                Edit = new PropertySecurityPermission(prop, edit.Name, editDenyReason);
            }
            else
            {
                // Property accepts input from clients.

                var roles = read.RoleLists.Concat(edit.RoleLists).ToList();
                Init = new PropertySecurityPermission(prop, "Init", roles, IsInitUnused);

                if (prop.IsInitOnly)
                {
                    Edit = new PropertySecurityPermission(prop, edit.Name, "Property uses an init accessor.");
                }
                else
                {
                    Edit = new PropertySecurityPermission(prop, edit.Name, roles, IsEditUnused);
                }
            }

            Restrictions = prop.GetAttributes<Attribute>()
                .Select(a => a.Type.GenericArgumentsFor(typeof(RestrictAttribute<>))?[0])
                .Where(a => a != null)
                .ToArray()!;
        }

        /// <summary>
        /// Security applied when sending a value to the client.
        /// </summary>
        public PropertySecurityPermission Read { get; }

        /// <summary>
        /// Security applied when accepting a value from the client onto a new instance of the object.
        /// </summary>
        public PropertySecurityPermission Init { get; }

        /// <summary>
        /// Security applied when accepting a value from the client onto an existing instance of the object.
        /// </summary>
        public PropertySecurityPermission Edit { get; }

        /// <summary>
        /// Dynamic restrictions to be applied for both Reads and Writes.
        /// </summary>
        public IReadOnlyList<TypeViewModel> Restrictions { get; }

        /// <summary>
        /// If true, the user can read the field.
        /// </summary>
        [Obsolete("This method cannot account for any custom IPropertyRestrictions.")]
        public bool IsReadAllowed(ClaimsPrincipal? user) => Read.IsAllowed(user);

        public bool IsReadAllowed(IMappingContext mappingContext, object model)
        {
            if (!Read.IsAllowed(mappingContext.User)) return false;

            return Prop.SecurityInfo.Restrictions.All(r => mappingContext
                .GetPropertyRestriction(r.TypeInfo)
                .UserCanRead(mappingContext, Prop.Name, model)
            );
        }

        /// <summary>
        /// If true, the user can initialize the field on a new instance of the object.
        /// </summary>
        [Obsolete("This method cannot account for any custom IPropertyRestrictions.")]
        public bool IsInitAllowed(ClaimsPrincipal? user) => Init.IsAllowed(user);

        /// <summary>
        /// If true, the user can edit the field on an existing instance of the object.
        /// </summary>
        [Obsolete("This method cannot account for any custom IPropertyRestrictions.")]
        public bool IsEditAllowed(ClaimsPrincipal? user) => Edit.IsAllowed(user);

        /// <inheritdoc cref="IsFilterAllowed(IMappingContext)"/>
        public bool IsFilterAllowed(CrudContext context) => IsFilterAllowed(context.MappingContext);

        /// <summary>
        /// If true, the user can sort/search/filter the field.
        /// </summary>
        public bool IsFilterAllowed(IMappingContext context)
        {
            if (!Read.IsAllowed(context.User)) return false;

            if (Restrictions.Count > 0)
            {
                foreach (var restrictionType in Restrictions)
                {
                    if (!context.GetPropertyRestriction(restrictionType.TypeInfo).UserCanFilter(context, Prop.Name))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool? IsReadUnused(ValueViewModel value, HashSet<PropertyViewModel> visited)
        {
            // The client only needs to Edit/Write to parameters, so read IS unused.
            if (value is ParameterViewModel) return true;
            // The client needs to Read from method returns, so read is not unused.
            if (value is MethodReturnViewModel) return false;

            if (value is PropertyViewModel p)
            {
                if (!visited.Add(p)) return null;
                if (!p.SecurityInfo.Read.IsAllowed()) return true;

                // Check for recursive usages where the action might be conclusively used.
                foreach (var usage in p.EffectiveParent.Usages)
                {
                    if (IsReadUnused(usage, visited) == false) return false;
                }

                // If nothing was found recusively, use the API permissions of the type if it has a CRUD api.
                if (p.EffectiveParent is { IsDbMappedType: true } or { IsStandaloneEntity: true })
                {
                    return p.EffectiveParent.SecurityInfo.Read.NoAccess;
                }
            }
            return true;
        }

        private static bool? IsInitUnused(ValueViewModel value, HashSet<PropertyViewModel> visited)
        {
            // The client needs to instantiate parameters, so edit is NOT unused.
            if (value is ParameterViewModel) return false;
            // The client only needs to Read from method returns, so edit IS unused.
            if (value is MethodReturnViewModel) return true;

            if (value is PropertyViewModel p)
            {
                if (!visited.Add(p)) return null;
                if (!p.SecurityInfo.Init.IsAllowed()) return true;

                // Check for recursive usages where the action might be conclusively used.
                foreach (var usage in p.EffectiveParent.Usages)
                {
                    if (IsInitUnused(usage, visited) == false) return false;
                }

                // If nothing was found recusively, use the API permissions of the type if it has a CRUD api.
                if (p.EffectiveParent.Type.IsA(typeof(IDataSource<>)) && p.HasAttribute<CoalesceAttribute>())
                {
                    // Property is a data source parameter. 
                    return false;
                }
                else if (p.EffectiveParent is { IsDbMappedType: true } or { IsStandaloneEntity: true })
                {
                    return p.EffectiveParent.SecurityInfo.Create.NoAccess;
                }
            }
            return true;
        }

        private static bool? IsEditUnused(ValueViewModel value, HashSet<PropertyViewModel> visited)
        {
            // Parameters always instantiate new objects, so edit IS unused.
            if (value is ParameterViewModel) return true;
            // The client only needs to Read from method returns, so edit IS unused.
            if (value is MethodReturnViewModel) return true;

            if (value is PropertyViewModel p)
            {
                if (!visited.Add(p)) return null;
                if (!p.SecurityInfo.Edit.IsAllowed()) return true;

                // Check for recursive usages where the action might be conclusively used.
                foreach (var usage in p.EffectiveParent.Usages)
                {
                    if (IsEditUnused(usage, visited) == false) return false;
                }

                // If nothing was found recusively, use the API permissions of the type if it has a CRUD api.
                if (p.EffectiveParent is { IsDbMappedType: true } or { IsStandaloneEntity: true })
                {
                    return p.EffectiveParent.SecurityInfo.Edit.NoAccess;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return $"Read:{Read}  Init:{Init}  Edit:{Edit}";
        }

    }
}
