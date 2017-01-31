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
    public class SecurityInfoClass
    {
        public SecurityInfoClass(SecurityInfoPermission read, SecurityInfoPermission edit,
            SecurityInfoPermission delete, SecurityInfoPermission create)
        {
            Read = read;
            Edit = edit;
            Delete = delete;
            Create = create;
        }

        public SecurityInfoPermission Read { get; set; }
        public SecurityInfoPermission Edit { get; set; }
        public SecurityInfoPermission Delete { get; set; }
        public SecurityInfoPermission Create { get; set; }


        public string ClassAnnotation
        {
            get
            {
                if (AllowAnonymousAll()) return "[AllowAnonymous]";

                //if (AllHaveRoles()) return $"[Authorize(Roles=\"{AllRoles()}\")]";

                return "[Authorize]";
            }
        }

        /// <summary>
        /// Returns an annotation for reading things (List/Get)
        /// </summary>
        public string ReadAnnotation
        {
            get
            {
                if (AllowAnonymousAll()) return string.Empty;
                if (Read.AllowAnonymous || Edit.AllowAnonymous || Create.AllowAnonymous || Delete.AllowAnonymous) return "[AllowAnonymous]";
                if (Read.HasRoles) return $"[Authorize(Roles=\"{AllRoles()}\")]";

                return "[Authorize]";
            }
        }

        ///// <summary>
        ///// Returns an annotation for editing things (Modify many/many properties)
        ///// </summary>
        public string EditAnnotation
        {
            get
            {
                if (AllowAnonymousAll()) return string.Empty;
                if (Edit.AllowAnonymous) return "[AllowAnonymous]";
                if (Edit.HasRoles) return $"[Authorize(Roles=\"{Edit.ExternalRoleList}\")]";

                return "[Authorize]";
            }
        }

        ///// <summary>
        ///// Returns an annotation for deleting things (Delete)
        ///// </summary>
        public string DeleteAnnotation
        {
            get
            {
                if (AllowAnonymousAll()) return string.Empty;
                if (Delete.AllowAnonymous) return "[AllowAnonymous]";
                if (Delete.HasRoles) return $"[Authorize(Roles=\"{Delete.ExternalRoleList}\")]";

                return "[Authorize]";
            }
        }

        ///// <summary>
        ///// Returns an annotation for editing things (Save)
        ///// </summary>
        public string SaveAnnotation
        {
            get
            {
                if (AllowAnonymousAll()) return string.Empty;
                if (Create.AllowAnonymous || Edit.AllowAnonymous) return "[AllowAnonymous]";

                if (Create.HasRoles && Edit.HasRoles)
                {
                    var roles = SaveRoles();
                    if (!string.IsNullOrEmpty(roles))
                        return $"[Authorize(Roles=\"{roles}\")]";
                }

                return "[Authorize]";
            }
        }

        public bool IsReadAllowed(ClaimsPrincipal user = null)
        {
            if (Read.HasAttribute)
            {
                if (AllowAnonymousAny) return true;
                if (Read.HasRoles && user != null)
                    return Read.RoleList.Any(s => user.IsInRole(s));
            }
            return user == null || user.Identity.IsAuthenticated;
        }

        public bool IsCreateAllowed(ClaimsPrincipal user = null)
        {
            if (Create.HasAttribute)
            {
                if (Create.NoAccess) return false;
                if (Create.AllowAnonymous) return true;
                if (Create.HasRoles && user != null)
                    return Create.RoleList.Any(s => user.IsInRole(s));
            }

            return user == null || user.Identity.IsAuthenticated;
        }

        public bool IsDeleteAllowed(ClaimsPrincipal user = null)
        {
            if (Delete.HasAttribute)
            {
                if (Delete.NoAccess) return false;
                if (Delete.AllowAnonymous) return true;
                if (Delete.HasRoles && user != null)
                    return Delete.RoleList.Any(s => user.IsInRole(s));
            }

            return user == null || user.Identity.IsAuthenticated;
        }

        public bool IsEditAllowed(ClaimsPrincipal user = null)
        {
            if (Edit.HasAttribute)
            {
                if (Edit.NoAccess) return false;
                if (Edit.AllowAnonymous) return true;
                if (Edit.HasRoles && user != null)
                    return Edit.RoleList.Any(s => user.IsInRole(s));
            }

            return user == null || user.Identity.IsAuthenticated;
        }

        public bool AllowAnonymousAny
        {
            get
            {
                return Read.AllowAnonymous || Edit.AllowAnonymous || Delete.AllowAnonymous || Create.AllowAnonymous;
            }
        }

        public bool AllowAnonymousAll()
        {
            return Read.AllowAnonymous && Edit.AllowAnonymous && Delete.AllowAnonymous && Create.AllowAnonymous;
        }


        private bool AllHaveRoles()
        {
            return Read.HasRoles && Edit.HasRoles && Create.HasRoles && Delete.HasRoles;
        }

        private string AllRoles()
        {
            var result = Read.RoleList;
            result.AddUnique(Edit.RoleList);
            result.AddUnique(Create.RoleList);
            result.AddUnique(Delete.RoleList);

            if (result.Count == 0) return "";
            return string.Join(",", result);
        }

        private string SaveRoles()
        {
            var result = Edit.RoleList;
            result.AddUnique(Create.RoleList);

            if (result.Count == 0) return string.Empty;
            return string.Join(",", result);
        }

        public override string ToString()
        {
            return Read.ToString() + Edit.ToString() + Delete.ToString() + Create.ToString();
        }

    }
}
