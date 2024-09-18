using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Coalesce.Starter.Vue.Web.Auth;

public class SecurityStampValidator(IOptions<SecurityStampValidatorOptions> options, SignInManager<User> signInManager, ILoggerFactory logger, AppDbContext db) : SecurityStampValidator<User>(options, signInManager, logger)
{
    public override Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        db.TenantId = context.Principal?.GetTenantId();
        return base.ValidateAsync(context);
    }
}
