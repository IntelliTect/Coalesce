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
    public class SecurityInfoProperty
    {
        public bool IsEdit { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public string EditRoles { get; set; } = "";
        public string ReadRoles { get; set; } = "";



        public List<string> EditRolesList { get
            {
                List<String> result = new List<string>();
                if (!string.IsNullOrEmpty(EditRoles))
                {
                    var roles = EditRoles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (roles.Length > 0)
                    {
                        foreach (var item in roles)
                        {
                            if (!result.Contains(item)) result.AddUnique(RoleMapping.Map(item));
                        }
                    }
                }
                return result;
            }
        }


        /// <summary>
        /// This is the read plus the edit lists.
        /// </summary>
        public List<string> ReadRolesList
        {
            get
            {
                var result = EditRolesList;

                if (!string.IsNullOrEmpty(ReadRoles))
                {
                    var roles = ReadRoles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (roles.Length > 0)
                    {
                        foreach (var item in roles)
                        {
                            if (!result.Contains(item)) result.AddUnique(RoleMapping.Map(item));
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Returns true for a property if this property has some sort of security.
        /// </summary>
        /// <returns></returns>
        public bool IsSecuredProperty
        {
            get
            {
                return IsEdit || IsRead;
            }
        }

        /// <summary>
        /// If true, the user can edit the field.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsEditable(ClaimsPrincipal user)
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
        public bool IsReadable(ClaimsPrincipal user)
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
                catch(Exception ex)
                {
                    // This happens when the trust is lost between the client and the domain.
                    if (ex.Message.Contains("trust")) return false;
                    throw new Exception("Could not authenticate with roles list", ex);
                }
            }
            return true;
        }

        public string ReadRolesExternal
        {
            get
            {
                return string.Join(",", ReadRolesList);
            }
        }

        public string EditRolesExternal
        {
            get
            {
                return string.Join(",", EditRolesList);
            }
        }



        public override string ToString()
        {
            string result = "";
            if (IsRead) { result += $"Read: {ReadRolesExternal} "; }
            if (IsEdit) { result += $"Edit: {EditRolesExternal} "; }
            if (result == "") result = "None";
            return result;
        }

    }
}
