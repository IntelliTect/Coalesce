using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IntelliTect.Coalesce.DataAnnotations
{
    public class SecurityInfoMethod
    {
        public SecurityInfoMethod(bool isExecute, string executeRoles)
        {
            HasAttribute = isExecute;
            ExecuteRoles = executeRoles;
        }

        public bool HasAttribute { get; }

        public string ExecuteRoles { get; } = "";


        public IReadOnlyCollection<string> ExecuteRolesList
        {
            get
            {
                List<string> result = new List<string>();
                if (!string.IsNullOrEmpty(ExecuteRoles))
                {
                    var roles = ExecuteRoles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    result.AddRange(roles.SelectMany(RoleMapping.Map).Distinct());
                }
                return result.AsReadOnly();
            }
        }

        public string ExecuteRolesExternal => string.Join(",", ExecuteRolesList);

        /// <summary>
        /// If true, the user can edit the field.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsExecutable(ClaimsPrincipal user)
        {
            if (!HasAttribute) return true;
            if (user == null) return false;
            if (string.IsNullOrEmpty(ExecuteRoles))
            {
                // No role checking.
                return true;
            }
            else
            {
                return ExecuteRolesList.Any(user.IsInRole);
            }
        }

        public override string ToString()
        {
            string result = "";
            if (HasAttribute) { result += $"Execute: {ExecuteRolesExternal} "; }
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
                if (!HasAttribute) return string.Empty; // Default to that of class
                if (!ExecuteRolesList.Any()) return ("[Authorize]");  // No roles specified if blank.
                return $"[Authorize(Roles=\"{string.Join(",", ExecuteRolesList)}\")]";
            }
        }
    }
}
