using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Models;

[Read(nameof(Permission.UserAdmin))]
[Edit(DenyAll)] // Doesn't make sense to edit. 
[Create(nameof(Permission.UserAdmin))]
[Delete(nameof(Permission.UserAdmin))]
public class UserRole : IdentityUserRole<string>
#if Tenancy
    , ITenanted
#endif
{
    // Fake PK for Coalesce since IdentityUserRole uses a composite PK.
    [NotMapped]
    public string Id
    {
        get => $"{UserId};{RoleId}";
        set
        {
            var split = value?.Split(';') ?? new string[2];
            UserId = split[0];
            RoleId = split[1];
        }
    }

#if Tenancy
    [InternalUse]
    [DefaultOrderBy(FieldOrder = 0)]
    [MaxLength(36)]
    public string TenantId { get; set; } = null!;
    [InternalUse]
    public Tenant? Tenant { get; set; }
#endif

    [DefaultOrderBy(FieldOrder = 0)]
    public User? User { get; set; }

    [DefaultOrderBy(FieldOrder = 1)]
    public Role? Role { get; set; }

    [DefaultDataSource]
    public class DefaultSource(CrudContext<AppDbContext> context) : AppDataSource<UserRole>(context)
    {
        // Teach Coalesce how to properly query for our compound key:
        protected override Task<UserRole?> EvaluateItemQueryAsync(
            object id,
            IQueryable<UserRole> query,
            CancellationToken cancellationToken = default)
        {
            var idParts = id.ToString()!.Split(";");
            return query.FirstOrDefaultAsync(r => r.UserId == idParts[0] && r.RoleId == idParts[1], cancellationToken);
        }
    }

    public class Behaviors(
        CrudContext<AppDbContext> context,
        SignInManager<User> signInManager
    ) : AppBehaviors<UserRole>(context)
    {
        public override ItemResult BeforeDelete(UserRole item)
        {
            // Check if deleting this UserRole would leave no admin users
            if (item.Role?.Permissions?.Contains(Permission.UserAdmin) == true)
            {
                if (WouldLeaveNoAdmins(excludeUserRoleId: item.Id))
                {
                    return "You cannot remove this user from the admin role as it would leave no remaining user admins.";
                }
            }

            return base.BeforeDelete(item);
        }

        public override async Task<ItemResult<UserRole>> AfterSaveAsync(SaveKind kind, UserRole? oldItem, UserRole item)
        {
            if (User.GetUserId() == item.Id)
            {
                // If the user was editing their own roles, refresh their current sign-in immediately
                // so that it doesn't feel like nothing happened.
                await signInManager.RefreshSignInAsync(item.User!);
            }

            return true;
        }

        private bool WouldLeaveNoAdmins(string excludeUserRoleId)
        {
            // Parse the composite ID to get UserId and RoleId
            var idParts = excludeUserRoleId.Split(';');
            if (idParts.Length != 2) return false;
            
            var excludeUserId = idParts[0];
            var excludeRoleId = idParts[1];

            var query = Db.Users.Where(u => u.UserRoles!.Any(ur => 
                !(ur.UserId == excludeUserId && ur.RoleId == excludeRoleId) && 
                ur.Role!.Permissions!.Contains(Permission.UserAdmin)));

#if Tenancy
            // In tenancy mode, only consider users in the current tenant
            query = query.Where(u => Db.TenantMemberships.Any(tm => tm.UserId == u.Id && tm.TenantId == User.GetTenantId()));
#endif

            return !query.Any();
        }
    }
}