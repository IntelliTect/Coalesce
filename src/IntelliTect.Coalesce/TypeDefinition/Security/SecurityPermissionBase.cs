using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class SecurityPermissionBase
    {
        public string Roles { get; protected set; } = "";
        public string Name { get; protected set; } = "";
        public bool HasRoles => RoleList.Count > 0;

        private IReadOnlyList<string>? _roleList;
        public IReadOnlyList<string> RoleList
        {
            get
            {
                if (_roleList != null) return _roleList;

                var list = new List<string>();
                if (!string.IsNullOrEmpty(Roles))
                {
                    // Logic here should mirror ASP.NET Core:
                    // split on commas, then trim each item.
                    // https://github.com/dotnet/aspnetcore/blob/d88935709b8908f371aa97e32a3ce4a74af2368f/src/Security/Authorization/Core/src/AuthorizationPolicy.cs#L155
                    var roles = Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    list.AddRange(roles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct());
                }
                return _roleList = list.AsReadOnly();
            }
        }

        public string ToStringWithName() => $"{Name}: {ToString()}";
    }
}