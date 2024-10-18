using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace Coalesce.Starter.Vue.Data.Models;

[Edit(AllowAuthenticated)]
[Create(DenyAll)]
[Delete(DenyAll)]
[Description("A user profile within the application.")]
public class User : IdentityUser
{
    [Search(SearchMethod = SearchMethods.Contains)]
    [ListText]
    public string? FullName { get; set; }

    [Search]
    public override string? UserName { get; set; }

    [Read] // Email readonly - sourced from external identity providers
    public override string? Email { get; set; }

    [Read]
    public override bool EmailConfirmed { get; set; }

#if UserPictures
    [Read, Hidden]
    public byte[]? PhotoHash { get; set; }

    [InverseProperty(nameof(UserPhoto.User))]
    public UserPhoto? Photo { get; set; }
#endif



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

    [InternalUse]
    public override string? NormalizedEmail { get; set; }

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
    [Hidden]
    public ICollection<UserRole>? UserRoles { get; set; }

    [Display(Name = "Roles")]
    [Read(nameof(Permission.UserAdmin))]
    public IEnumerable<string>? RoleNames => UserRoles?.Where(ur => ur.Role != null).Select(r => r.Role!.Name!);

#if Tenancy
    [Read(AppClaimValues.GlobalAdminRole)]
    [Edit(AppClaimValues.GlobalAdminRole)]
    [Hidden]
    [Description("Global admins can perform some administrative actions against ALL tenants.")]
    public bool IsGlobalAdmin { get; set; }
#endif

#if UserPictures
    [Coalesce, Execute(HttpMethod = HttpMethod.Get, VaryByProperty = nameof(PhotoHash))]
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
    public static async Task<ItemResult> InviteUser(
        AppDbContext db,
        [Inject] InvitationService invitationService,
        [DataType(DataType.EmailAddress)] string email,
        Role? role
    )
    {
        Role[] roles = role is null ? [] : [role];
        return await invitationService.CreateAndSendInvitation(email, roles);
    }
#endif
#endif

#if (LocalAuth)
    [Coalesce]
    public async Task<ItemResult> SetEmail(
        [Inject] UserManagementService userService,
        ClaimsPrincipal currentUser,
        [DataType(DataType.EmailAddress)] string newEmail
    )
    {
        if (currentUser.GetUserId() != this.Id && !currentUser.Can(Permission.UserAdmin)) return "Unauthorized.";
        return await userService.SendEmailChangeRequest(this, newEmail);
    }

    [Coalesce]
    public async Task<ItemResult> SendEmailConfirmation(
        [Inject] UserManagementService userService,
        ClaimsPrincipal currentUser
    )
    {
        if (currentUser.GetUserId() != this.Id && !currentUser.Can(Permission.UserAdmin)) return "Unauthorized.";
        return await userService.SendEmailConfirmationRequest(this);
    }

    [Coalesce]
    public async Task<ItemResult> SetPassword(
        [Inject] UserManager<User> userManager,
        [Inject] SignInManager<User> signInManager,
        ClaimsPrincipal currentUser,
        [DataType(DataType.Password)] string? currentPassword,
        [DataType(DataType.Password)] string newPassword,
        [DataType(DataType.Password)] string confirmNewPassword
    )
    {
        if (currentUser.GetUserId() != this.Id) return "Unauthorized.";

        if (newPassword != confirmNewPassword) return "New passwords must match";

        var result = this.PasswordHash is null
            ? await userManager.AddPasswordAsync(this, newPassword)
            : await userManager.ChangePasswordAsync(this, currentPassword ?? "", newPassword);

        if (!result.Succeeded)
        {
            return string.Join("; ", result.Errors.Select(e => e.Description));
        }

        if (currentUser.GetUserId() == this.Id)
        {
            await signInManager.RefreshSignInAsync(this);
        }
        return new ItemResult(true, $"Password was successfully changed.");
    }
#endif

    [InternalUse]
    public void InitializeFirstUser(AppDbContext db)
    {
#if Tenancy
        // If this user is the first user, make them the global admin
        this.IsGlobalAdmin = true;

#if (!TenantCreateSelf && !TenantCreateExternal)
        // Ensure that the very first user belongs to a tenant so they can create more tenants.
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Name == "Demo Tenant");
        if (tenant is not null) 
        {
            db.TenantId = tenant.TenantId;
            db.TenantMemberships.Add(new() { TenantId = tenant.TenantId, User = this });
            this.UserRoles = db.Roles.Select(r => new UserRole { Role = r, User = this }).ToList();
            logger.LogInformation($"Granting demo tenant membership for initial user {this.Id}");
        }
#endif
#else
        // If this user is the first user, give them all roles so there is an initial admin.
        this.UserRoles = db.Roles.Select(r => new UserRole { Role = r, User = this }).ToList();
#endif
    }

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
            if (item.Id != User.GetUserId()) return "Forbidden.";
#else
            // Users who aren't user admins can only edit their own profile.
            if (item.Id != User.GetUserId() && !User.Can(Permission.UserAdmin)) return "Forbidden.";
#endif

            if (item.UserName != oldItem?.UserName)
            {
                if (Db.Users.Any(u => u.UserName == item.UserName && u.Id != item.Id))
                {
                    return "Username is already taken.";
                }

                item.NormalizedUserName = userManager.NormalizeName(item.UserName);
            }

            if (oldItem != null)
            {
#if (!Tenancy)
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
#endif
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
