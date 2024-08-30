using Microsoft.AspNetCore.Identity;

namespace Coalesce.Starter.Vue.Data.Models;

[Edit("UserAdmin")]
[Read]
[Create(DenyAll)]
[Delete(DenyAll)]
public class AppUser : IdentityUser
{
    [Search(SearchMethod = SearchMethods.Contains)]
    public string? FullName { get; set; }

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
    [InverseProperty(nameof(AppUserRole.User))]
    [ManyToMany("Roles")]
    public ICollection<AppUserRole>? UserRoles { get; set; }

}