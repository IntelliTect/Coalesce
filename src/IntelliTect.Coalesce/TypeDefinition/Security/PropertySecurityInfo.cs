using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
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
        public PropertySecurityInfo(SecurityPermission read, SecurityPermission edit)
        {
            IsRead = !read.NoAccess;
            ReadRoles = read.Roles;

            if (read.HasAttribute && !edit.HasAttribute)
            {
                IsEdit = false;
                EditRoles = "";
            }
            else
            {
                IsEdit = !edit.NoAccess;
                EditRoles = edit.Roles;
            }

            Read = new PropertySecurityPermission(
                allow: !read.NoAccess,
                roles: read.Roles,
                name: read.Name
            );

            Edit = new PropertySecurityPermission(
                allow:
                    // A [Read] attribute without an [Edit] signifies read-only:
                    read.HasAttribute && !edit.HasAttribute ? false :
                    !edit.NoAccess,
                roles: edit.Roles,
                name: edit.Name
            );
        }

        public bool IsEdit { get; }
        public bool IsRead { get; }
        public string EditRoles { get; }
        public string ReadRoles { get; }

        public PropertySecurityPermission Read { get; }
        public PropertySecurityPermission Edit { get; }

        public IReadOnlyList<string> EditRolesList => Edit.RoleList;

        public IReadOnlyList<string> ReadRolesList => Read.RoleList;

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
