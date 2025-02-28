using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Coalesce.Starter.Vue.Web.Pages;

#nullable disable

[AllowAnonymous]
public class ForgotPasswordModel(
    UserManager<User> userManager, 
    UserManagementService userManagementService
) : PageModel
{
    public bool Success { get; set; }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Username { get; set; }

#nullable restore

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        User? user = await userManager.FindByNameAsync(Username);
        await userManagementService.SendPasswordResetRequest(user);

        Success = true;
        return Page();
    }
}
