﻿using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// <para>
    /// When placed on an entity or custom <see cref="IClassDto{T}"/> class exposed by Coalesce,
    /// controls the permissions for the deleting existing instances of the model via the /delete or /bulkSave endpoints.
    /// </para>
    /// </summary>      
    [AttributeUsage(AttributeTargets.Class)]
    public class DeleteAttribute : SecurityAttribute
    {
        public DeleteAttribute()
        {
        }

        public DeleteAttribute(SecurityPermissionLevels permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public DeleteAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
