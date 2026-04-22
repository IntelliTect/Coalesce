using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace Coalesce.Starter.Vue.Web.Pages;

[AllowAnonymous]
[EnableRateLimiting("auth")]
public class RegisterModel(
    AppDbContext db,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    UserManagementService userManagementService
) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

#if Passwords
    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
#endif

    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = new User()
        {
            UserName = Email,
            Email = Email
        };

        new DatabaseSeeder(db).InitializeFirstUser(user);

#if Passwords
        var result = await userManager.CreateAsync(user, Password);
#else
        var result = await userManager.CreateAsync(user);
#endif

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        var emailResult = await userManagementService.SendEmailConfirmationRequest(user);
        if (userManager.Options.SignIn.RequireConfirmedAccount)
        {
            SuccessMessage = emailResult.Message;
            return Page();
        }
        else
        {
            await signInManager.SignInAsync(user, isPersistent: false);
#if Passkeys
            return LocalRedirect(Url.Page("/CreatePasskey", values: new { ReturnUrl })!);
#else
            return LocalRedirect(ReturnUrl ?? "/");
#endif
        }
    }
}
