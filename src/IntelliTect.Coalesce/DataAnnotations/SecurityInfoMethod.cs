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
    public class SecurityInfoMethod
    {
        public bool IsExecute { get; set; } = false;
        public string ExecuteRoles { get; set; } = "";



        public List<string> ExecuteRolesList
        {
            get
            {
                List<String> result = new List<string>();
                if (!string.IsNullOrEmpty(ExecuteRoles))
                {
                    var roles = ExecuteRoles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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

        public string ExecuteRolesExternal
        {
            get
            {
                return string.Join(",", ExecuteRolesList);
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
                return IsExecute;
            }
        }

        /// <summary>
        /// If true, the user can edit the field.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsExecutable(ClaimsPrincipal user)
        {
            if (!IsSecuredProperty) return true;
            if (user == null) return false;
            if (string.IsNullOrEmpty(ExecuteRoles))
            {
                // No role checking.
                return true;
            }
            else
            {
                return ExecuteRolesList.Any(f => user.IsInRole(f));
            }
        }

        public override string ToString()
        {
            string result = "";
            if (IsExecute) { result += $"Execute: {ExecuteRolesExternal} "; }
            if (result == "") result = "None";
            return result;
        }


        /// <summary>
        /// Returns an annotation for reading things (List/Get)
        /// </summary>
        public string ExecuteAnnotation
        {
            get
            {
                if (!IsExecute) return ""; // Default to that of class
                if (String.IsNullOrEmpty(ExecuteRolesExternal)) return ("[Authorize]");  // No roles specified if blank.
                return $"[Authorize]";
            }
        }
    }
}
