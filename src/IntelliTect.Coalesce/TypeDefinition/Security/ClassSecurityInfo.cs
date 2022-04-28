﻿using IntelliTect.Coalesce.DataAnnotations;
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

            var readAttribute = classViewModel.GetSecurityPermission<ReadAttribute>();
            var editAttribute = classViewModel.GetSecurityPermission<EditAttribute>();
            var deleteAttribute = classViewModel.GetSecurityPermission<DeleteAttribute>();
            var createAttribute = classViewModel.GetSecurityPermission<CreateAttribute>();

            var notMutable = false;
            if (ClassViewModel.IsStandaloneEntity)
            {
                // Standalone entities are only mutable if they have a behaviors class.
                // (since their backing store is up to the developer).
                // Everything else falls back on StandardBehaviors (or user-configured DefaultBehaviors)
                notMutable = ClassViewModel.ReflectionRepository?.GetBehaviorsDeclaredFor(ClassViewModel) == null;
            }

            if (!ClassViewModel.WillCreateApiController)
            {
                notMutable = true;
            }

            var allowAnonymousAny = readAttribute.AllowAnonymous || editAttribute.AllowAnonymous || deleteAttribute.AllowAnonymous || createAttribute.AllowAnonymous;

            Read = new SecurityPermission(
                level:
                    !ClassViewModel.WillCreateApiController ? SecurityPermissionLevels.DenyAll :
                    readAttribute.NoAccess ? SecurityPermissionLevels.DenyAll :
                    allowAnonymousAny ? SecurityPermissionLevels.AllowAll :
                    SecurityPermissionLevels.AllowAuthorized,
                roles: readAttribute.HasRoles 
                    ? string.Join(",", readAttribute.RoleList.Union(editAttribute.RoleList).Union(createAttribute.RoleList).Union(deleteAttribute.RoleList)) 
                    : null,
                name: readAttribute.Name
            );

            Edit = new SecurityPermission(
                level: 
                    notMutable ? SecurityPermissionLevels.DenyAll : 
                    editAttribute.PermissionLevel,
                roles: editAttribute.Roles,
                name: editAttribute.Name
            );

            Create = new SecurityPermission(
                level: 
                    notMutable ? SecurityPermissionLevels.DenyAll : 
                    createAttribute.PermissionLevel,
                roles: createAttribute.Roles,
                name: createAttribute.Name
            );

            Save = new SecurityPermission(
                level: 
                    notMutable ? SecurityPermissionLevels.DenyAll :
                    createAttribute.NoAccess && editAttribute.NoAccess ? SecurityPermissionLevels.DenyAll :
                    createAttribute.AllowAnonymous || editAttribute.AllowAnonymous ? SecurityPermissionLevels.AllowAll :
                    SecurityPermissionLevels.AllowAuthorized,
                roles: createAttribute.HasRoles && editAttribute.HasRoles 
                    ? string.Join(",", editAttribute.RoleList.Union(createAttribute.RoleList).ToList())
                    : null,
                name: "Save"
            );

            Delete = new SecurityPermission(
                level: 
                    notMutable ? SecurityPermissionLevels.DenyAll : 
                    deleteAttribute.PermissionLevel,
                roles: deleteAttribute.Roles,
                name: deleteAttribute.Name
            );
        }

        public ClassViewModel ClassViewModel { get; }

        public SecurityPermission Read { get; }
        public SecurityPermission Create { get; }
        public SecurityPermission Edit { get; }
        public SecurityPermission Save { get; }
        public SecurityPermission Delete { get; }

        /// <summary>
        /// Return whether general reading is allowed, without taking into account any particular user.
        /// </summary>
        public bool IsReadAllowed() => Read.IsAllowed();

        /// <summary>
        /// Return whether reading is allowed for the specified user.
        /// </summary>
        public bool IsReadAllowed(ClaimsPrincipal? user) => Read.IsAllowed(user);

        /// <summary>
        /// Return whether general creating is allowed, without taking into account any particular user.
        /// </summary>
        public bool IsCreateAllowed() => Create.IsAllowed();

        /// <summary>
        /// Return whether creating is allowed for the specified user.
        /// </summary>
        public bool IsCreateAllowed(ClaimsPrincipal? user) => Create.IsAllowed(user);

        /// <summary>
        /// Return whether general editing is allowed, without taking into account any particular user.
        /// </summary>
        public bool IsEditAllowed() => Edit.IsAllowed();

        /// <summary>
        /// Return whether editing is allowed for the specified user.
        /// </summary>
        public bool IsEditAllowed(ClaimsPrincipal? user) => Edit.IsAllowed(user);

        /// <summary>
        /// Return whether general saving (create or edit) is allowed, without taking into account any particular user.
        /// </summary>
        public bool IsSaveAllowed() => Save.IsAllowed();

        /// <summary>
        /// Return whether saving (create or edit) is allowed for the specified user.
        /// </summary>
        public bool IsSaveAllowed(ClaimsPrincipal? user) => Save.IsAllowed(user);

        /// <summary>
        /// Return whether general deleting is allowed, without taking into account any particular user.
        /// </summary>
        public bool IsDeleteAllowed() => Delete.IsAllowed();

        /// <summary>
        /// Return whether deleting is allowed for the specified user.
        /// </summary>
        public bool IsDeleteAllowed(ClaimsPrincipal? user) => Delete.IsAllowed(user);

        public override string ToString()
        {
            return $"Read:{Read}  Edit:{Edit}  Delete:{Delete}  Create:{Create}";
        }

    }
}
