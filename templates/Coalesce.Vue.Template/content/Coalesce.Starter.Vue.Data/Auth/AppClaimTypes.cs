namespace Coalesce.Starter.Vue.Data;

public static class AppClaimTypes
{
#if Tenancy
    public const string GlobalAdminRole = "GlobalAdmin";
#endif

    public const string Role = "role";
    public const string Permission = "perm";
    public const string UserId = "sub";
    public const string UserName = "username";
    public const string Email = "email";
    public const string FullName = "name";
#if Tenancy
    public const string TenantId = "tid";
#endif
}