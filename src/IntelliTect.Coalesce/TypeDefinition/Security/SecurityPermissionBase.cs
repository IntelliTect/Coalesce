using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IntelliTect.Coalesce.TypeDefinition;

public abstract class SecurityPermissionBase
{
    internal string Roles
    {
        get
        {
            if (RoleLists.Count > 1)
            {
                throw new InvalidOperationException("Multiple role lists cannot be represented in a single string. Read from RoleLists instead.");
            }
            if (RoleLists.Count == 0) return "";
            return string.Join(",", RoleLists.Single());
        }
        set
        {
            RoleLists = new List<IReadOnlyList<string>>
            {
                SplitRoles(value)
            }.AsReadOnly();
        }
    }

    internal IReadOnlyList<string> RoleList
    {
        get
        {
            if (RoleLists.Count > 1)
            {
                throw new InvalidOperationException("Multiple role lists cannot be represented in a single RoleList. Read from RoleLists instead.");
            }
            return RoleLists.FirstOrDefault() ?? new List<string>().AsReadOnly();
        }
    }

    public string Name { get; protected set; } = "";
    public string Reason { get; protected set; } = "";
    public bool HasRoles => RoleLists.Any(l => l.Count > 0);

    public bool NoAccess { get; protected set; }

    /// <summary>
    /// Sets of roles required for the permission.
    /// For each outer list, the user must be a member of one of the roles in the inner list.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> RoleLists { get; protected set; } = NoRoles;

    // For inclusion of the string representation when serializing
    public string Display => ToString() ?? "";

    public string ToStringWithName() => $"{Name}: {ToString()}";

    /// <summary>
    /// Return whether the action is generally allowed, without taking into account any particular user.
    /// </summary>
    public bool IsAllowed() => !NoAccess;

    /// <summary>
    /// Return whether the action is allowed for the specified user.
    /// </summary>
    public abstract bool IsAllowed(ClaimsPrincipal? user);


    protected static readonly IReadOnlyList<IReadOnlyList<string>> NoRoles = new List<IReadOnlyList<string>>().AsReadOnly();

    internal static IReadOnlyList<string> SplitRoles(string? roles)
    {
        if (string.IsNullOrEmpty(roles)) return new List<string>().AsReadOnly();

        // Logic here should mirror ASP.NET Core:
        // split on commas, then trim each item.
        // https://github.com/dotnet/aspnetcore/blob/d88935709b8908f371aa97e32a3ce4a74af2368f/src/Security/Authorization/Core/src/AuthorizationPolicy.cs#L155
        return roles
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct()
            .ToList()
            .AsReadOnly();
    }
}
