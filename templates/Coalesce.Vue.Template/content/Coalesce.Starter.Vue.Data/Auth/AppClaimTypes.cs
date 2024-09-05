namespace Coalesce.Starter.Vue.Data;

public static class AppClaimTypes
{
    public const string Role = "role";
    public const string Permission = "perm";
    public const string UserId = ClaimTypes.NameIdentifier;
    public const string UserName = ClaimTypes.Name;
    public const string Email = ClaimTypes.Email;
    public const string FullName = nameof(FullName);
}