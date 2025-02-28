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
    }
}