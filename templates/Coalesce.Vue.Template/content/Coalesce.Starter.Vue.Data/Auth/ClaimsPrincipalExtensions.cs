namespace Coalesce.Starter.Vue.Data.Auth;

public static class ClaimsPrincipalExtensions
{
#if Identity
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(AppClaimTypes.UserId);

    public static string? GetUserName(this ClaimsPrincipal user)
        => user.FindFirstValue(AppClaimTypes.UserName);

    public static string? GetEmail(this ClaimsPrincipal user)
        => user.FindFirstValue(AppClaimTypes.Email);

    public static bool Can(this ClaimsPrincipal user, params Permission[] permissions)
        => permissions.Any(p => user.HasClaim(AppClaimTypes.Permission, p.ToString()));

#else

    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static string? GetUserName(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Name);
#endif
#if Tenancy
    public static int? GetTenantId(this ClaimsPrincipal user)
        => user.FindFirstValue(AppClaimTypes.TenantId) is string s
            ? int.Parse(s)
            : null;
#endif
}