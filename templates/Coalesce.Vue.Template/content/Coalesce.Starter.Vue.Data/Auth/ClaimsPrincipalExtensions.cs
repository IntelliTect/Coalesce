namespace Coalesce.Starter.Vue.Data.Utilities;

public static class ClaimsPrincipalExtensions
{
    public static bool Can(this ClaimsPrincipal user, params Permission[] permissions)
        => permissions.Any(p => user.HasClaim(AppClaimTypes.Permission, p.ToString()));
}