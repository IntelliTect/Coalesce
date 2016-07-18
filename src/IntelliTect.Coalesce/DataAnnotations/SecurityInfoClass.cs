using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Class that contains security information for a class or property based on the Read and Edit attributes
    /// </summary>
    public class SecurityInfoClass
    {
        public bool IsEdit { get; set; } = false;
        public bool IsRead { get; set; } = false;
        public bool AllowAnonymousEdit { get; set; } = false;
        public bool AllowAnonymousRead { get; set; } = false;
        public string EditRoles { get; set; } = "";
        public string ReadRoles { get; set; } = "";

        public string ClassAnnotation
        {
            get
            {
                if (!IsEdit && !IsRead) return "[Authorize]"; // Default to closed
                if (String.IsNullOrEmpty(AllRoles)) return ("[Authorize]");  // No roles specified if blank.
                if (AllowAnonymousEdit && AllowAnonymousRead) return "[AllowAnonymous]";
                return $"[Authorize(Roles=\"{AllRoles}\")]";
            }
        }
        /// <summary>
        /// Returns an annotation for reading things (List/Get)
        /// </summary>
        public string ReadAnnotation
        {
            get
            {
                if (AllowAnonymousEdit || AllowAnonymousRead) return "[AllowAnonymous]";
                if (!IsRead && !IsRead) return "[Authorize]"; // Default to closed
                if (String.IsNullOrEmpty(AllRoles)) return ("[Authorize]");  // No roles specified if blank.
                return $"[Authorize(Roles=\"{AllRoles}\")]";
            }
        }
        /// <summary>
        /// Returns an annotation for editing things (Save/Delete)
        /// </summary>
        public string EditAnnotation
        {
            get
            {
                if (AllowAnonymousEdit) return "[AllowAnonymous]";
                if (!IsEdit) return "[Authorize]"; // Default to closed
                if (String.IsNullOrEmpty(EditRolesExternal)) return ("[Authorize]");  // No roles specified if blank.
                return $"[Authorize(Roles=\"{EditRolesExternal}\")]";
            }
        }

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

        public List<string> ReadRolesList
        {
            get
            {
                List<String> result = new List<string>();
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



        public string AllRoles
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
                            if (!result.Contains(item)) result.Add(item);
                        }
                    }
                }
                if (result.Count == 0) return "";
                return string.Join(",", result);
            }
        }

        public override string ToString()
        {
            string result = "";
            if (IsRead) {
                if (AllowAnonymousRead) result += "Read: Anonymous ";
                else result += $"Read: {ReadRolesExternal} ";
            }
            if (IsEdit) {
                if (AllowAnonymousEdit) result += "Edit: Anonymous ";
                else result += $"Edit: {EditRolesExternal} "; }
            if (result == "") result = "Authorized";
            return result;
        }


    }
}
