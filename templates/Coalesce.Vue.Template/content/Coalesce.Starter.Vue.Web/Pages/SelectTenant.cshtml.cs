using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using Coalesce.Starter.Vue.Data.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Coalesce.Starter.Vue.Web.Pages;

[Authorize]
public class SelectTenantModel(AppDbContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public List<Tenant> Tenants { get; private set; } = [];

    public async Task OnGet()
    {
        await LoadTenants();
    }

    public async Task<IActionResult> OnPost(
        [FromForm] string tenantId,
        [FromServices] SignInManager<User> signInManager
    )
    {
        await LoadTenants();
        if (!Tenants.Any(t => t.TenantId == tenantId))
        {
            ModelState.AddModelError("tenantId", "Invalid Tenant");
        }
        if (!ModelState.IsValid) return Page();

        db.ForceSetTenant(tenantId);

        var user = await db.Users.FindAsync(User.GetUserId());
        await signInManager.RefreshSignInAsync(user!);

        return LocalRedirect(string.IsNullOrWhiteSpace(ReturnUrl) ? "/" : ReturnUrl);
    }

    private async Task LoadTenants()
    {
        var userId = User.GetUserId();
        Tenants = await db.TenantMemberships
            .IgnoreTenancy()
            .Where(tm => tm.UserId == userId)
            .Select(tm => tm.Tenant!)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
