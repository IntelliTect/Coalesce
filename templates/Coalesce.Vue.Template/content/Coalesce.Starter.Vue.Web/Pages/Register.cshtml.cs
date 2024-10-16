using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Coalesce.Starter.Vue.Web.Pages;

[AllowAnonymous]
public class RegisterModel(
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = new User()
        {
            UserName = Email,
            Email = Email
        };

        var result = await userManager.CreateAsync(user, Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await userManagementService.SendEmailConfirmationRequest(user);
        if (userManager.Options.SignIn.RequireConfirmedAccount)
        {
            return RedirectToPage("RegisterConfirmation", new { email = Email, returnUrl = ReturnUrl });
        }
        else
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(ReturnUrl ?? "/");
        }
    }
}
