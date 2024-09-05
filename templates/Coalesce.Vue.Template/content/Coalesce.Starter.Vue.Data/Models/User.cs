using Coalesce.Starter.Vue.Data.Coalesce;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Edit(nameof(Permission.UserAdmin))]
[Create(DenyAll)]
[Delete(DenyAll)]
[Description("A user profile within the application.")]
public class User : IdentityUser
{
    [Search(SearchMethod = SearchMethods.Contains)]
    public string? FullName { get; set; }

#if UserPictures
    [Read, Hidden]
    public byte[]? PhotoMD5 { get; set; }

    [InverseProperty(nameof(UserPhoto.User))]
    public UserPhoto? Photo { get; set; }
#endif

    [Search]
    public override string? UserName { get; set; }

    [InternalUse]
	public override string? NormalizedUserName { get; set; }

	[InternalUse]
	public override string? PasswordHash { get; set; }

	[InternalUse]
	public override string? SecurityStamp { get; set; }

	[InternalUse]
	public override string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

	[InternalUse]
	public override string? PhoneNumber { get; set; }
    [InternalUse]
    public override bool PhoneNumberConfirmed { get; set; }

    [Read(nameof(Permission.UserAdmin)), Edit(nameof(Permission.UserAdmin))]
    public override string? Email { get; set; }
    [InternalUse]
    public override string? NormalizedEmail { get; set; }
    [InternalUse]
    public override bool EmailConfirmed { get; set; }

	[InternalUse]
	public override bool TwoFactorEnabled { get; set; }

#if PasswordAuth
    [Read(nameof(Permission.UserAdmin))]
    [Description("The number of failed login attempts for the user. Reset after a successful password sign-in.")]
#else
    [InternalUse]
#endif
    public override int AccessFailedCount { get; set; }

    [Description("If set, the user will be blocked from signing in until this date.")]
    [Read(nameof(Permission.UserAdmin)), Edit(nameof(Permission.UserAdmin))]
    public override DateTimeOffset? LockoutEnd { get; set; }

#if PasswordAuth
    [Description("If enabled, the user will be locked out after multiple failed sign-in attempts.")]
#else
    [Description("If enabled, the user can be locked out.")]
#endif
    [Read(nameof(Permission.UserAdmin)), Edit(nameof(Permission.UserAdmin))]
    public override bool LockoutEnabled { get; set; }

    [Read(nameof(Permission.UserAdmin))]
    [InverseProperty(nameof(UserRole.User))]
    [ManyToMany("Roles")]
    public ICollection<UserRole>? UserRoles { get; set; }

    [Read(nameof(Permission.UserAdmin))]
    [NotMapped, DataType(DataType.MultilineText)]
    [Hidden(HiddenAttribute.Areas.List)]
    [Display(Description = "A summary of the effective permissions of the user, derived from their current roles.")]
    public string? EffectivePermissions
    {
        get
        {
            var currentPermissions = UserRoles
                ?.SelectMany(u => u.Role?.Permissions!.Select(p => (p, u.Role)) ?? [])
                .ToLookup(p => p.p, p => p.Role);

            if (currentPermissions is null) return null;

            return string.Join("\n", Enum.GetValues<Permission>().Select(p => currentPermissions.Contains(p)
                ? $"✅ {p.GetDisplayName()} (via {string.Join(", ", currentPermissions[p].Select(r => r.Name))})"
                : $"❌ {p.GetDisplayName()}"
            ));
        }
    }

#if UserPictures
    [Coalesce, Execute(HttpMethod = HttpMethod.Get, VaryByProperty = nameof(PhotoMD5))]
    public ItemResult<IFile> GetPhoto(AppDbContext db)
    {
        return new IntelliTect.Coalesce.Models.File(db.UserPhotos
            .Where(p => p.UserId == this.Id)
            .Select(p => p.Content))
        {
            ContentType = "image/*"
        };
    }
#endif

    public class UserBehaviors(
        CrudContext<AppDbContext> context, 
        UserManager<User> userManager,
        SignInManager<User> signInManager
    ) : AppBehaviors<User>(context)
    {
        public override ItemResult BeforeSave(SaveKind kind, User? oldItem, User item)
        {
            if (oldItem != null)
            {
                if (item.Email != oldItem.Email)
                {
                    item.NormalizedEmail = userManager.NormalizeEmail(item.Email);
                    item.SecurityStamp = Guid.NewGuid().ToString();
                }

                if (item.UserName != oldItem.UserName)
                {
                    item.NormalizedUserName = userManager.NormalizeEmail(item.UserName);
                    item.SecurityStamp = Guid.NewGuid().ToString();
                }

                if (item.LockoutEnd != oldItem.LockoutEnd)
                {
                    // Auto-enable lockout when setting a lockout date.
                    if (item.LockoutEnd != null) item.LockoutEnabled = true;

                    // Invalidate existing sessions when manually locking a user's account.
                    item.SecurityStamp = Guid.NewGuid().ToString();
                }

                if (!item.LockoutEnabled)
                {
                    // Make it clear to the administrator that lockout is only respected when LockoutEnabled.
                    item.LockoutEnd = null;
                }
            }

            return base.BeforeSave(kind, oldItem, item);
        }

        public override async Task<ItemResult<User>> AfterSaveAsync(SaveKind kind, User? oldItem, User item)
        {
            if (User.GetUserId() == item.Id)
            {
                // If the user was editing their own profile,
                // refresh their current sign-in so they aren't kicked out if
                // the change required a refresh to the user's security stamp.
                await signInManager.RefreshSignInAsync(item);
            }

            return await base.AfterSaveAsync(kind, oldItem, item);
        }
    }
}
