using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    public class ClassSecurityInfo
    {
        public ClassSecurityInfo(ClassViewModel classViewModel)
        {
            ClassViewModel = classViewModel;
            Read = classViewModel.GetSecurityPermission<ReadAttribute>();
            Edit = classViewModel.GetSecurityPermission<EditAttribute>();
            Delete = classViewModel.GetSecurityPermission<DeleteAttribute>();
            Create = classViewModel.GetSecurityPermission<CreateAttribute>();
        }

        public ClassViewModel ClassViewModel { get; }
        public SecurityPermission Read { get; }
        public SecurityPermission Edit { get; }
        public SecurityPermission Delete { get; }
        public SecurityPermission Create { get; }


        public string ClassAnnotation
        {
            get
            {
                if (AllowAnonymousAll) return "[AllowAnonymous]";

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
                if (Read.NoAccess) throw NoAccessException();
                if (AllowAnonymousAll) return string.Empty;
                if (Read.AllowAnonymous || Edit.AllowAnonymous || Create.AllowAnonymous || Delete.AllowAnonymous) return "[AllowAnonymous]";
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
                if (Edit.HasRoles) return $"[Authorize(Roles=\"{Edit.AttributeRoleList}\")]";

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
                if (Delete.HasRoles) return $"[Authorize(Roles=\"{Delete.AttributeRoleList}\")]";

                return "[Authorize]";
            }
        }

        /// <summary>
        /// Returns an annotation for editing things (Save)
        /// </summary>
        public string SaveAnnotation
        {
            get
            {
                if (Create.NoAccess && Edit.NoAccess) throw NoAccessException();
                if (AllowAnonymousAll) return string.Empty;
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

        /// <summary>
        /// This is checked, and this exception thrown, to prevent accidents in code generation.
        /// </summary>
        /// <returns></returns>
        private Exception NoAccessException()
            => new InvalidOperationException(
                $"Cannot emit an annotation for permission level {SecurityPermissionLevels.DenyAll}. Templates shouldn't emit anything in such cases.");

        public bool IsReadAllowed(ClaimsPrincipal? user = null)
        {
            if (Read.HasAttribute)
            {
                if (Read.NoAccess) return false;
                if (AllowAnonymousAny) return true;
                if (Read.HasRoles && user != null)
                    return Read.RoleList.Any(s => user.IsInRole(s));
            }
            return user == null || (user.Identity?.IsAuthenticated ?? false);
        }

        public bool IsCreateAllowed(ClaimsPrincipal? user = null) => IsMutationActionAllowed(Create, user);

        public bool IsDeleteAllowed(ClaimsPrincipal? user = null) => IsMutationActionAllowed(Delete, user);

        public bool IsEditAllowed(ClaimsPrincipal? user = null) => IsMutationActionAllowed(Edit, user);

        private bool IsMutationActionAllowed(SecurityPermission action, ClaimsPrincipal? user = null)
        {
            if (ClassViewModel.IsStandaloneEntity && 
                ClassViewModel.ReflectionRepository?.GetBehaviorsDeclaredFor(ClassViewModel) == null)
            {
                return false;
            }

            if (action.HasAttribute)
            {
                if (action.NoAccess) return false;
                if (action.AllowAnonymous) return true;
                if (action.HasRoles && user != null)
                    return action.RoleList.Any(s => user.IsInRole(s));
            }

            return user == null || (user.Identity?.IsAuthenticated ?? false);
        }

        public bool AllowAnonymousAny => Read.AllowAnonymous || Edit.AllowAnonymous || Delete.AllowAnonymous || Create.AllowAnonymous;

        public bool AllowAnonymousAll => Read.AllowAnonymous && Edit.AllowAnonymous && Delete.AllowAnonymous && Create.AllowAnonymous;

        private string AllRoles()
        {
            var result = Read.RoleList
                .Union(Edit.RoleList)
                .Union(Create.RoleList)
                .Union(Delete.RoleList)
                .ToList();

            if (result.Count == 0) return "";
            return string.Join(",", result);
        }

        private string SaveRoles()
        {
            var result = Edit.RoleList
                .Union(Create.RoleList)
                .ToList();

            if (result.Count == 0) return string.Empty;
            return string.Join(",", result);
        }

        public override string ToString()
        {
            return $"Read:{Read}  Edit:{Edit}  Delete:{Delete}  Create:{Create}";
        }

    }
}
