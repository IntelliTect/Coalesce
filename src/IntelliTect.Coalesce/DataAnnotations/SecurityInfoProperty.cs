using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Class that contains security information for a class or property based on the Read and Edit attributes
    /// </summary>
    public class PropertySecurityInfo
    {
        public PropertySecurityInfo(SecurityPermission read, SecurityPermission edit,
            SecurityPermission delete)
        {
            Read = read;
            Edit = edit;
            Delete = delete;

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
        }
        public bool IsEdit { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public string EditRoles { get; set; } = "";
        public string ReadRoles { get; set; } = "";

        public SecurityPermission Read { get; set; }
        public SecurityPermission Edit { get; set; }
        public SecurityPermission Delete { get; set; }

        public List<string> EditRolesList
        {
            get
            {
                if (!string.IsNullOrEmpty(EditRoles))
                {
                    return EditRoles
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct()
                        .ToList();
                }
                return new List<string>();
            }
        }


        public List<string> ReadRolesList
        {
            get
            {
                if (!string.IsNullOrEmpty(ReadRoles))
                {
                    return ReadRoles
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct()
                        .ToList();
                }
                return new List<string>();
            }
        }

        /// <summary>
        /// Returns true for a property if this property has some sort of security.
        /// </summary>
        /// <returns></returns>
        public bool IsSecuredProperty => IsEdit || IsRead;

        /// <summary>
        /// If true, the user can edit the field.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsEditable(ClaimsPrincipal? user)
        {
            if (!IsSecuredProperty) return true;
            if (string.IsNullOrEmpty(EditRoles))
            {
                // This field just has a read on it, so no one can edit it.
                return false;
            }
            else
            {
                if (user == null || !EditRolesList.Any(f => user.IsInRole(f)))
                {
                    return false;
                }
            }
            return true;
        }

        // If true, the user can view the property
        public bool IsReadable(ClaimsPrincipal? user)
        {
            if (!IsSecuredProperty) return true;
            if (!string.IsNullOrEmpty(ReadRoles))
            {
                try
                {
                    if (user == null || !ReadRolesList.Any(f => user.IsInRole(f)))
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // This happens when the trust is lost between the client and the domain.
                    if (ex.Message.Contains("trust")) return false;
                    throw new Exception("Could not authenticate with roles list", ex);
                }
            }
            return true;
        }


        public string ReadRolesExternal => string.Join(",", ReadRolesList);

        public string EditRolesExternal => string.Join(",", EditRolesList);


        public override string ToString()
        {
            string result = "";
            if (IsRead) { result += $"Read: {ReadRoles} "; }
            if (IsEdit) { result += $"Edit: {EditRoles} "; }
            if (result == "") result = "None";
            return result;
        }

    }
}
