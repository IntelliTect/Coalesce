using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace Coalesce.Starter.Vue.Data.Auth;

public class UserManagementService(
    UserManager<User> _userManager,
    IUrlHelper urlHelper,
    IEmailSender emailSender
)
{
    public async Task<ItemResult> SendEmailConfirmationRequest(User user)
    {
        if (user.EmailConfirmed) return "Email is already confirmed.";
        if (string.IsNullOrWhiteSpace(user.Email)) return "User has no email";

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var link = urlHelper.PageLink("/ConfirmEmail", values: new { userId = user.Id, code = code })!;

        // TODO: Generalize this into the email sending abstractions
        if (emailSender is NoOpEmailSender && Debugger.IsAttached) Debugger.Break(); // DEVELOPMENT: Grab the value of `link`.

        await emailSender.SendEmailAsync(
            user.Email,
            "Confirm your email",
            $"""
            Please <a href='{HtmlEncoder.Default.Encode(link)}'>click here</a> to confirm your account.
            If you didn't request this, ignore this email and do not click the link.
            """
        );

        // todo: handle failure
        var itemResult = new ItemResult(true, $"An email was sent to {user.Email}. Please click the link in the email to confirm your account.");
        return itemResult;
    }

    public async Task<ItemResult> SendEmailChangeRequest(User user, string newEmail)
    {
        // This is secured by virtue of the filtering done in the [DefaultDataSource].
        // Regular users can only fetch themselves out of the data source,
        // admins can only view users in their own tenant,
        // and tenant admins can view everyone.
        
        if (string.IsNullOrEmpty(newEmail) || !MailAddress.TryCreate(newEmail, out _))
        {
            return "New email is not valid.";
        }

        if (string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            return "New email is not different.";
        }

        var code = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        var link = urlHelper.PageLink("/ConfirmEmail", values: new { userId = user.Id, code = code, newEmail = newEmail })!;

        await emailSender.SendEmailAsync(
            newEmail, 
            "Confirm your email",
            $"""
            Please <a href='{HtmlEncoder.Default.Encode(link)}'>click here</a> to complete your email change request.
            If you didn't request this, ignore this email and do not click the link.
            """
        );

        // todo: handle failure
        var itemResult = new ItemResult(true, $"An email was sent to {newEmail}. Please click the link in the email to complete the change.");
        return itemResult;
    }

    public async Task<ItemResult> SendPasswordResetRequest(User? user)
    {
        if (user?.Email is not null)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var link = urlHelper.PageLink("ResetPassword", values: new { userId = user.Id, code = code })!;

            // todo: handle failure
            await emailSender.SendEmailAsync(
                user.Email,
                "Password Reset",
                $"""
                Please <a href='{HtmlEncoder.Default.Encode(link)}'>click here</a> to reset your password.
                If you didn't request this, ignore this email and do not click the link.
                """
            );
        }

        return new ItemResult(true,
            "If the provided user account exists, the email address on the account " +
            "will be receiving an email shortly with directions to reset your password.");
    }
}
