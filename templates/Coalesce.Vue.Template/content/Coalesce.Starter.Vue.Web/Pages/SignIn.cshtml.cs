using Coalesce.Starter.Vue.Data.Models;
#if (Passwords || Passkeys)
using Coalesce.Starter.Vue.Data.Auth;
#endif
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Coalesce.Starter.Vue.Web.Pages;

[AllowAnonymous]
public class SignInModel(
    SignInManager<User> signInManager
#if (Passwords || Passkeys)
    , UserManager<User> userManager,
    UserManagementService userManagementService
#endif
) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

#if (Passwords || Passkeys)
    /// <summary>What step of the sign-in flow are we on.</summary>
    public int Step { get; set; } = 1;

    /// <summary>Whether a one-time code was just sent to the user's email.</summary>
    public bool CodeSent { get; set; }

    [BindProperty]
    public string? Username { get; set; }

    [BindProperty]
    public string? Code { get; set; }

    [BindProperty]
    public string? Action { get; set; }
#endif

#if Passwords
    [BindProperty]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
#endif

#if Passkeys
    [BindProperty]
    public string? CredentialJson { get; set; }
#endif

    public void OnGet()
    {
    }

#if (Passwords || Passkeys)
    public async Task<IActionResult> OnPostAsync()
    {
#if Passkeys
        // Passkey sign-in can happen from any step (including conditional mediation from step 1)
        if (!string.IsNullOrEmpty(CredentialJson))
        {
            var result = await signInManager.PasskeySignInAsync(CredentialJson);
            if (result.Succeeded)
            {
                return LocalRedirect(ReturnUrl ?? "/");
            }

            Step = string.IsNullOrWhiteSpace(Username) ? 1 : 2;

            ModelState.AddModelError(
                string.Empty,
                result.IsLockedOut
                    ? "Account locked out"
                    : result.IsNotAllowed
                        ? "This account is not allowed to sign in."
                        : "Invalid passkey.");
            return Page();
        }
#endif

        if (string.IsNullOrWhiteSpace(Username))
        {
            ModelState.AddModelError(nameof(Username), "Username is required.");
            return Page();
        }

        var user = await userManager.FindByNameAsync(Username);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            Step = 1;
            return Page();
        }

        // Step 1 -> Step 2: Username entered, show authentication methods.
        if (Action == "continue")
        {
            Step = 2;
            return Page();
        }

        // Send a one-time sign-in code to the user's email.
        if (Action == "sendCode")
        {
            Step = 2;

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError(string.Empty, "User does not have a valid email address.");
                return Page();
            }

            var sendCodeResult = await userManagementService.SendSignInCode(user);
            if (!sendCodeResult.WasSuccessful)
            {
                ModelState.AddModelError(string.Empty, sendCodeResult.Message ?? "Failed to send sign-in code.");
                return Page();
            }
            CodeSent = true;
            return Page();
        }

        // Verify one-time code.
        if (!string.IsNullOrWhiteSpace(Code))
        {
            if (await userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Account locked out.");
                Step = 2;
                CodeSent = true;
                return Page();
            }

            var isValid = await userManager.VerifyUserTokenAsync(
                user, TokenOptions.DefaultEmailProvider, "SignIn", Code!);
            if (isValid)
            {
                await userManager.ResetAccessFailedCountAsync(user);
                await signInManager.SignInAsync(user, true);
#if Passkeys
                return LocalRedirect(Url.Page("/CreatePasskey", values: new { ReturnUrl })!);
#else
                return LocalRedirect(ReturnUrl ?? "/");
#endif
            }

            await userManager.AccessFailedAsync(user);
            ModelState.AddModelError(string.Empty, "Invalid or expired code.");
            Step = 2;
            CodeSent = true;
            return Page();
        }

#if Passwords
        // Password sign-in.
        if (!string.IsNullOrWhiteSpace(Password))
        {
            var result = await signInManager.PasswordSignInAsync(Username, Password, true, true);
            if (result.Succeeded)
            {
#if Passkeys
                return LocalRedirect(Url.Page("/CreatePasskey", values: new { ReturnUrl })!);
#else
                return LocalRedirect(ReturnUrl ?? "/");
#endif
            }

            ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Account locked out" : "Invalid login attempt.");
            Step = 2;
            return Page();
        }
#endif

        // Fallback: show step 2.
        Step = 2;
        return Page();
    }
#endif
}
