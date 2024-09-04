using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Models;

[Edit(nameof(Permission.UserAdmin))]
[Create(DenyAll)]
[Delete(DenyAll)]
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

    [InternalUse]
	public override string? Email { get; set; }
    [InternalUse]
    public override string? NormalizedEmail { get; set; }
    [InternalUse]
    public override bool EmailConfirmed { get; set; }

	[InternalUse]
	public override bool TwoFactorEnabled { get; set; }

	[Read("UserAdmin")]
	public override int AccessFailedCount { get; set; }

	[Read("UserAdmin")]
	public override DateTimeOffset? LockoutEnd { get; set; }

	[Read("UserAdmin")]
	public override bool LockoutEnabled { get; set; }

    [Read("UserAdmin")]
    [InverseProperty(nameof(UserRole.User))]
    [ManyToMany("Roles")]
    public ICollection<UserRole>? UserRoles { get; set; }

#if UserPictures
    [Coalesce, Execute(HttpMethod = HttpMethod.Get, VaryByProperty = nameof(PhotoMD5))]
    public ItemResult<IFile> GetPhoto(AppDbContext db)
    {
        return new IntelliTect.Coalesce.Models.File(db.UserPhotos
            .Where(p => p.UserId == this.Id)
            .Select(p => p.Content));
    }
#endif

    // TODO: Make first user an admin.
    // TODO: Seed an admin role with the admin permissions
}
