using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Coalesce.Starter.Vue.Web.Pages
{
    [Authorize]
    public class CreateTenantModel(AppDbContext db) : PageModel
    {
        [Required]
        [BindProperty]
        [Display(Name = "Organization Name")]
        public string? Name { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(
            [FromServices] SignInManager<User> signInManager
        )
        {
            if (!ModelState.IsValid) return Page();

            Tenant tenant = new() { Name = Name! };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();

            db.ResetToTenant(tenant.TenantId);
            new DatabaseSeeder(db).SeedNewTenant(tenant, User.GetUserId());

            // Sign the user into the new tenant (uses `db.TenantId`).
            var user = await db.Users.FindAsync(User.GetUserId());
            await signInManager.RefreshSignInAsync(user!);

            return Redirect("/");
        }
    }
}
