using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Coalesce.Starter.Vue.Web.Pages;

#nullable disable

[AllowAnonymous]
public class ResetPasswordModel(UserManager<User> userManager, SignInManager<User> signInManager) : PageModel
{
    public const string InvalidError = "The link is no longer valid.";

    public bool Success { get; set; }

    [BindProperty(SupportsGet = true)]
    [Required]
    public string Code { get; set; }

    [BindProperty(SupportsGet = true)]
    [Required]
    public string UserId { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string Password { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await userManager.FindByIdAsync(UserId);
        if (user == null)
        {
            ModelState.AddModelError("", InvalidError);
            return Page();
        }

        var result = await userManager.ResetPasswordAsync(user, Code, Password);
        if (result.Succeeded)
        {
            Success = true;
            await signInManager.SignOutAsync();
            return Page();
        }

        foreach (var error in result.Errors)
        {
            string description = error.Description;
            if (error.Code == new IdentityErrorDescriber().InvalidToken().Code)
            {
                // Default error for an expired/invalid link is not user-friendly.
                description = InvalidError;
            }
            ModelState.AddModelError(string.Empty, description);
        }
        return Page();
    }
}
