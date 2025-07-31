namespace Coalesce.Starter.Vue.Data;

public static class AppClaimTypes
{
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

#if Tenancy
public static class AppClaimValues
{
    /// <summary>
    /// Global admins can perform some administrative actions against ALL tenants.
    /// This role is a special role granted by <see cref="User.IsGlobalAdmin" />.
    /// </summary>
    public const string GlobalAdminRole = "GlobalAdmin";
    
    public const string NullTenantId = "00000000-0000-0000-0000-000000000000";
}
#endif