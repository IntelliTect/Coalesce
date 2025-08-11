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

            // Prevent removing UserAdmin permission if it would leave no user admins
            if (kind == SaveKind.Update && oldItem != null)
            {
                var oldHadUserAdmin = oldItem.Permissions?.Contains(Permission.UserAdmin) == true;
                var newHasUserAdmin = item.Permissions?.Contains(Permission.UserAdmin) == true;
                
                if (oldHadUserAdmin && !newHasUserAdmin)
                {
                    var result = CheckWouldLeaveNoUserAdmins(item.Id);
                    if (!result.WasSuccessful) return result;
                }
            }

            item.NormalizedName = roleManager.NormalizeKey(item.Name);

            return base.BeforeSave(kind, oldItem, item);
        }

        public override ItemResult BeforeDelete(Role item)
        {
            // Prevent deleting role with UserAdmin permission if it would leave no user admins
            if (item.Permissions?.Contains(Permission.UserAdmin) == true)
            {
                var result = CheckWouldLeaveNoUserAdmins(item.Id);
                if (!result.WasSuccessful) return result;
            }

            return base.BeforeDelete(item);
        }

        private ItemResult CheckWouldLeaveNoUserAdmins(string roleIdToExclude)
        {
            // Count users who have UserAdmin permission through roles other than the one being modified/deleted
            var adminUserCount = Db.Users
                .Where(u => u.UserRoles!.Any(ur => 
                    ur.RoleId != roleIdToExclude && 
                    ur.Role!.Permissions!.Contains(Permission.UserAdmin)))
                .Count();

            if (adminUserCount == 0)
            {
                return "This action would leave the system with no user administrators. At least one user admin must remain.";
            }

            return true;
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
