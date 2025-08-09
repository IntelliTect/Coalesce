using Coalesce.Starter.Vue.Data.Coalesce;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Create(nameof(Permission.UserAdmin))]
[Edit(nameof(Permission.UserAdmin))]
[Delete(nameof(Permission.UserAdmin))]
[Description("Roles are groups of permissions, analogous to job titles or functions.")]
public class Role
#if Tenancy
    : IdentityRole, ITenanted
#else
    : IdentityRole
#endif
{
#if Tenancy
    [InternalUse]
    [DefaultOrderBy(FieldOrder = 0)]
    [MaxLength(36)]
    public string TenantId { get; set; } = null!;
    [InternalUse]
    [ForeignKey(nameof(TenantId))]
    public Tenant? Tenant { get; set; }
#endif

    [Required, Search(SearchMethod = SearchMethods.Contains)]
    public override string? Name { get; set; }

    [InternalUse]
    public override string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    [InternalUse]
    public override string? NormalizedName { get; set; }

    public List<Permission>? Permissions { get; set; }

    public class Behaviors(RoleManager<Role> roleManager, CrudContext<AppDbContext> context) : AppBehaviors<Role>(context)
    {
        public override ItemResult BeforeSave(SaveKind kind, Role? oldItem, Role item)
        {
#if Tenancy
            if (AppClaimValues.GlobalAdminRole.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
            {
                return $"{item.Name} is a reserved role name and cannot be used.";
            }
#endif

            // Check if removing UserAdmin permission would leave no admins
            if (kind == SaveKind.Update && oldItem != null)
            {
                var oldHasUserAdmin = oldItem.Permissions?.Contains(Permission.UserAdmin) == true;
                var newHasUserAdmin = item.Permissions?.Contains(Permission.UserAdmin) == true;
                
                if (oldHasUserAdmin && !newHasUserAdmin)
                {
                    // Check if this would leave no admin users
                    if (WouldLeaveNoAdmins(excludeRoleId: item.Id))
                    {
                        return "You cannot remove UserAdmin permission from this role as it would leave no remaining user admins.";
                    }
                }
            }

            item.NormalizedName = roleManager.NormalizeKey(item.Name);

            return base.BeforeSave(kind, oldItem, item);
        }

        public override ItemResult BeforeDelete(Role item)
        {
            // Check if deleting this role would leave no admin users
            if (item.Permissions?.Contains(Permission.UserAdmin) == true)
            {
                if (WouldLeaveNoAdmins(excludeRoleId: item.Id))
                {
                    return "You cannot delete this role as it would leave no remaining user admins.";
                }
            }

            return base.BeforeDelete(item);
        }

        private bool WouldLeaveNoAdmins(string excludeRoleId)
        {
            var query = Db.Users.Where(u => u.UserRoles!.Any(ur => 
                ur.RoleId != excludeRoleId && 
                ur.Role!.Permissions!.Contains(Permission.UserAdmin)));

#if Tenancy
            // In tenancy mode, only consider users in the current tenant
            query = query.Where(u => Db.TenantMemberships.Any(tm => tm.UserId == u.Id && tm.TenantId == User.GetTenantId()));
#endif

            return !query.Any();
        }
    }
}

#if Tenancy
[InternalUse]
public class RoleClaim : IdentityRoleClaim<string>, ITenanted
{
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }

    [InternalUse]
    [DefaultOrderBy(FieldOrder = 0)]
    [MaxLength(36)]
    public required string TenantId { get; set; }
    [InternalUse]
    [ForeignKey(nameof(TenantId))]
    public Tenant? Tenant { get; set; }
}
#else
[InternalUse]
public class RoleClaim : IdentityRoleClaim<string> 
{
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }
}
#endif