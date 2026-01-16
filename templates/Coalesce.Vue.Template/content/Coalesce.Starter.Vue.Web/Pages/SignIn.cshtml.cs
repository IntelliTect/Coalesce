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

#if LocalAuth
    [Required]
    [BindProperty]
    public required string Username { get; set; }

    [Required]
    [BindProperty]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

#if Passkeys
    [BindProperty]
    public string? CredentialJson { get; set; }
#endif

#endif
    public void OnGet()
    {
    }

#if LocalAuth
    public async Task<IActionResult> OnPostAsync()
    {

#if Passkeys
        Microsoft.AspNetCore.Identity.SignInResult result;
        if (!string.IsNullOrEmpty(CredentialJson))
        {
            result = await signInManager.PasskeySignInAsync(CredentialJson);
        }
        else
        {
            if (!ModelState.IsValid) return Page();
            result = await signInManager.PasswordSignInAsync(Username, Password, true, true);
        }
#else
        if (!ModelState.IsValid) return Page();

        var result = await signInManager.PasswordSignInAsync(Username, Password, true, true);
#endif

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl ?? "/");
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Account locked out" : "Invalid login attempt.");

        return Page();
    }
#endif
}