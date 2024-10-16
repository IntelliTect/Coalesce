using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using IntelliTect.Coalesce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coalesce.Starter.Vue.Web.Pages;

[AllowAnonymous]
public class ConfirmEmailModel(UserManager<User> userManager, SignInManager<User> signInManager) : PageModel
{
    public const string InvalidError = "The link is no longer valid.";

    public async Task<IActionResult> OnGetAsync(string userId, string code, string? newEmail)
    {
        if (
            string.IsNullOrWhiteSpace(userId) || 
            string.IsNullOrWhiteSpace(code) || 
            (await userManager.FindByIdAsync(userId)) is not { } user
        )
        {
            ModelState.AddModelError("", InvalidError);
            return Page();
        }

        var result = string.IsNullOrWhiteSpace(newEmail)
            ? await userManager.ConfirmEmailAsync(user, code)
            : await userManager.ChangeEmailAsync(user, newEmail, code);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", InvalidError);
            return Page();
        }

        if (User.GetUserId() == user.Id)
        {
            // The verifying user is already signed in. Refresh their session
            // so they see the new email.
            await signInManager.RefreshSignInAsync(user);
        }
        else
        {
            // A different user was signed in. Sign the user out so they don't get confused.
            await signInManager.SignOutAsync();
        }

        return Page();
    }
}
