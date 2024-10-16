using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Coalesce.Starter.Vue.Web.Pages;

[AllowAnonymous]
public class SignInModel(SignInManager<User> signInManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string? Provider { get; set; }

#if LocalAuth
    [Required]
    [BindProperty]
    public required string Username { get; set; }

    [Required]
    [BindProperty]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
#endif

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!string.IsNullOrWhiteSpace(Provider))
        {
            // Request a redirect to the external login provider.
            return new ChallengeResult(Provider, new()
            {
                RedirectUri = ReturnUrl
            });
        }

#if LocalAuth
        if (!ModelState.IsValid) return Page();

        var result = await signInManager.PasswordSignInAsync(Username, Password, true, true);
        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl ?? "/");
        }
        //else if (result.RequiresTwoFactor)
        //{
        //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = ReturnUrl });
        //}
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Account locked out" : "Invalid login attempt.");
#endif

        return Page();
    }
}
