using IntelliTect.Coalesce.Helpers;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

#if Tenancy
// Since users exist across tenants, a user may only edit their own profile.
// Admins within a particular tenant cannot edit the properties of a user
// that will affect other tenants.
[Edit(AllowAuthenticated)]
#else
[Edit(nameof(Permission.UserAdmin))]
#endif
[Create(DenyAll)]
[Delete(DenyAll)]
[Description("A user profile within the application.")]
public class User : IdentityUser
{
    [Search(SearchMethod = SearchMethods.Contains)]
    [Restrict<UserDataRestrictions>]
    public string? FullName { get; set; }

#if UserPictures
    [Read, Hidden]
    public byte[]? PhotoMD5 { get; set; }

    [InverseProperty(nameof(UserPhoto.User))]
    public UserPhoto? Photo { get; set; }
#endif

    [Search]
    [Restrict<UserDataRestrictions>]
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

    [Restrict<UserDataRestrictions>]
    public override string? Email { get; set; }

    [InternalUse]
    public override string? NormalizedEmail { get; set; }

    [Restrict<UserDataRestrictions>, ReadOnly(true)]
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
#if Tenancy
    [InternalUse]
#else
    [Read(nameof(Permission.UserAdmin)), Edit(nameof(Permission.UserAdmin))]
#endif
    public override DateTimeOffset? LockoutEnd { get; set; }

#if PasswordAuth
    [Description("If enabled, the user will be locked out after multiple failed sign-in attempts.")]
#else
    [Description("If enabled, the user can be locked out.")]
#endif
#if Tenancy
    [InternalUse]
#else
    [Read(nameof(Permission.UserAdmin)), Edit(nameof(Permission.UserAdmin))]
#endif
    public override bool LockoutEnabled { get; set; }

    [Read(nameof(Permission.UserAdmin), NoAutoInclude = true)]
    [InverseProperty(nameof(UserRole.User))]
    [ManyToMany("Roles")]
    public ICollection<UserRole>? UserRoles { get; set; }

#if Tenancy
    /// <summary>
    /// The user is a global administrator, able to perform administrative actions against all tenants.
    /// </summary>
    [Read(AppClaimValues.GlobalAdminRole)]
    [Edit(AppClaimValues.GlobalAdminRole)]
    [Hidden]
    public bool IsGlobalAdmin { get; set; }
#endif

#if UserPictures
    [Coalesce, Execute(HttpMethod = HttpMethod.Get, VaryByProperty = nameof(PhotoMD5))]
    public ItemResult<IFile> GetPhoto(ClaimsPrincipal user, AppDbContext db)
    {
        return new IntelliTect.Coalesce.Models.File(db.UserPhotos
            .Where(p => p.UserId == this.Id)
#if Tenancy
            .Where(p => db.TenantMemberships.Any(tm => tm.UserId == this.Id && tm.TenantId == user.GetTenantId()))
#endif
            .Select(p => p.Content))
        {
            ContentType = "image/*"
        };
    }
#endif

#if Tenancy
    [Coalesce, Execute(Roles = nameof(Permission.UserAdmin))]
    public ItemResult Evict(ClaimsPrincipal callingUser, AppDbContext db)
    {
        if (
            Id == callingUser.GetUserId() && 
            db.Users.Count(u => u.UserRoles!.Any(r => r.Role!.Permissions!.Contains(Permission.UserAdmin))) == 1
        )
        {
            return "You cannot remove the last remaining user admin.";
        }

        this.SecurityStamp = Guid.NewGuid().ToString();
        db.RemoveRange(db.UserRoles.Where(u => u.UserId == this.Id));
        db.RemoveRange(db.TenantMemberships.Where(u => u.UserId == this.Id));
        db.SaveChanges();

        return true;
    }

#if TenantMemberInvites
    [Coalesce, Execute(Roles = nameof(Permission.UserAdmin))]
    public static async Task<ItemResult> Invite(
        ClaimsPrincipal callingUser, 
        AppDbContext db,
        [Inject] InvitationService invitationService,
        [DataType(DataType.EmailAddress)] string email,
        Role? role
    )
    {
        Role[] roles = role is null ? [] : [role];

        var invitation = new UserInvitation
        {
            Email = email,
            Issued = DateTimeOffset.Now,
            Roles = roles.Select(r => r.Id).ToArray(),
            TenantId = db.TenantIdOrThrow
        };

        var user = await db.Users
            .Where(u => u.Email == email && u.EmailConfirmed)
            .FirstOrDefaultAsync();

        if (user is not null)
        {
            return await AcceptInvitation(db, invitation, user);
        }

        var link = invitationService.CreateInvitationLink(invitation);
        return new(true, link);
    }

    public static async Task<ItemResult> AcceptInvitation(
        AppDbContext db,
        UserInvitation invitation,
        User? acceptingUser
    )
    {
        var tenant = await db.Tenants.FindAsync(invitation.TenantId);

        if (acceptingUser is null) return "User not found";
        if (tenant is null) return "Tenant not found";

        db.TenantId = invitation.TenantId;

        if (await db.TenantMemberships.AnyAsync(m => m.User == acceptingUser))
        {
            return $"{acceptingUser.UserName ?? acceptingUser.Email} is already a member of {tenant.Name}.";
        }

        db.TenantMemberships.Add(new() { User = acceptingUser });
        db.UserRoles.AddRange(invitation.Roles.Select(rid => new UserRole { RoleId = rid, User = acceptingUser }));
        await db.SaveChangesAsync();

        return new(true, $"{acceptingUser.UserName ?? acceptingUser.Email} has been added as a member of {tenant.Name}.");
    }

#endif
#endif

    [DefaultDataSource]
    public class DefaultSource(CrudContext<AppDbContext> context) : AppDataSource<User>(context)
    {
        public override IQueryable<User> GetQuery(IDataSourceParameters parameters)
        {
            var query = base.GetQuery(parameters);
            if (User.Can(Permission.UserAdmin))
            {
                query = query.Include(u => u.UserRoles!).ThenInclude(ur => ur.Role);
            }

#if Tenancy
            return query.Where(u => Db.TenantMemberships.Any(tm => tm.UserId == u.Id && tm.TenantId == User.GetTenantId()));
#else
            return query;
#endif
        }
    }

    public class UserBehaviors(
        CrudContext<AppDbContext> context, 
        UserManager<User> userManager,
        SignInManager<User> signInManager
    ) : AppBehaviors<User>(context)
    {
        public override ItemResult BeforeSave(SaveKind kind, User? oldItem, User item)
        {
#if Tenancy
            // Since users exist across tenants, a user may only edit their own profile.
            // Admins within a particular tenant cannot edit the properties of a user
            // that will affect other tenants.
            if (item.Id != User.GetUserId() && !User.IsInRole(AppClaimValues.GlobalAdminRole)) return "Forbidden.";
#endif

            if (oldItem != null)
            {
                if (item.Email != oldItem.Email)
                {
                    item.NormalizedEmail = userManager.NormalizeEmail(item.Email);
                    item.EmailConfirmed = false;
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

public class UserDataRestrictions : IPropertyRestriction<User>
{
    public bool UserCanRead(IMappingContext context, string propertyName, User model)
    {
        if (context.User.GetUserId() == model.Id) return true;

        return UserCanFilter(context, propertyName);
    }

    public bool UserCanFilter(IMappingContext context, string propertyName)
    {
        return propertyName switch
        {
            nameof(User.FullName) => true,
            nameof(User.UserName) => true,
#if Tenancy
            _ => false
#else
        _ => context.User.Can(Permission.UserAdmin)
#endif
        };
    }

    public bool UserCanWrite(IMappingContext context, string propertyName, User? model, object? incomingValue)
    {
        if (model == null) return false; // Manual user creation isn't allowed.

        if (context.User.GetUserId() == model.Id) return propertyName switch
        {
            nameof(User.FullName) => true,
            _ => false
        };

#if Tenancy
        return false;
#else
        return context.User.Can(Permission.UserAdmin);
#endif
    }
}
