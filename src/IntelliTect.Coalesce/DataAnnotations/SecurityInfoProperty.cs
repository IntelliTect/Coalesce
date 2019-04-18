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
        }
        public bool IsEdit { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public string EditRoles { get; set; } = "";
        public string ReadRoles { get; set; } = "";

        public SecurityPermission Read { get; set; }
        public SecurityPermission Edit { get; set; }
        public SecurityPermission Delete { get; set; }

        public bool AllowAnonymousAny => Read.AllowAnonymous || Edit.AllowAnonymous || Delete.AllowAnonymous;

        public bool AllowAnonymousAll => Read.AllowAnonymous && Edit.AllowAnonymous && Delete.AllowAnonymous;


        public List<string> EditRolesList
        {
            get
            {
                if (!string.IsNullOrEmpty(EditRoles))
                {
                    return EditRoles
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .SelectMany(role => RoleMapping.Map(role))
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
                        .SelectMany(role => RoleMapping.Map(role))
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

        /// <summary>
        /// Returns an annotation for reading things (List/Get)
        /// </summary>
        public string ReadAnnotation
        {
            get
            {
                if (Read.NoAccess) throw NoAccessException();
                if (AllowAnonymousAll) return string.Empty;
                if (AllowAnonymousAny) return "[AllowAnonymous]";
                if (Read.HasRoles) return $"[Authorize(Roles=\"{AllRoles()}\")]";

                return "[Authorize]";
            }
        }

        /// <summary>
        /// Returns an annotation for editing things
        /// </summary>
        public string EditAnnotation
        {
            get
            {
                if (Edit.NoAccess) throw NoAccessException();
                if (AllowAnonymousAll) return string.Empty;
                if (Edit.AllowAnonymous) return "[AllowAnonymous]";
                if (Edit.HasRoles) return $"[Authorize(Roles=\"{Edit.ExternalRoleList}\")]";

                return "[Authorize]";
            }
        }

        /// <summary>
        /// Returns an annotation for deleting things (Delete)
        /// </summary>
        public string DeleteAnnotation
        {
            get
            {
                if (Delete.NoAccess) throw NoAccessException();
                if (AllowAnonymousAll) return string.Empty;
                if (Delete.AllowAnonymous) return "[AllowAnonymous]";
                if (Delete.HasRoles) return $"[Authorize(Roles=\"{Delete.ExternalRoleList}\")]";

                return "[Authorize]";
            }
        }

        /// <summary>
        /// This is checked, and this exception thrown, to prevent accidents in code generation.
        /// </summary>
        /// <returns></returns>
        private Exception NoAccessException()
            => new InvalidOperationException(
                $"Cannot emit an annotation for permission level {SecurityPermissionLevels.DenyAll}. Templates shouldn't emit anything in such cases.");

        private string AllRoles()
        {
            var result = Read.RoleList
                .Union(Delete.RoleList)
                .ToList();

            if (result.Count() == 0) return "";
            return string.Join(",", result);
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
