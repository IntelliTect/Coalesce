using Coalesce.Starter.Vue.Data.Communication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace Coalesce.Starter.Vue.Data.Auth;

public class UserManagementService(
    UserManager<User> userManager,
    IUrlHelper urlHelper,
    IEmailService emailSender
)
{
    public async Task<ItemResult> SendEmailConfirmationRequest(User user)
    {
        if (user.EmailConfirmed) return "Email is already confirmed.";
        if (string.IsNullOrWhiteSpace(user.Email)) return "User has no email";

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var link = urlHelper.PageLink("/ConfirmEmail", values: new { userId = user.Id, code = code })!;

        var result = await emailSender.SendEmailAsync(
            user.Email,
            "Confirm your email",
            $"""
            Please <a href="{HtmlEncoder.Default.Encode(link)}">click here</a> to confirm your account.
            If you didn't request this, ignore this email and do not click the link.
            """
        );

        if (result.WasSuccessful)
        {
            result.Message += " Please click the link in the email to confirm your account.";
        }

        return result;
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

        var code = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        var link = urlHelper.PageLink("/ConfirmEmail", values: new { userId = user.Id, code = code, newEmail = newEmail })!;

        var result = await emailSender.SendEmailAsync(
            newEmail,
            "Confirm your email",
            $"""
            Please <a href="{HtmlEncoder.Default.Encode(link)}">click here</a> to complete your email change request.
            If you didn't request this, ignore this email and do not click the link.
            """
        );

        if (result.WasSuccessful)
        {
            result.Message += " Please click the link in the email to complete the change.";
        }

        return result;
    }

    public async Task<ItemResult> SendPasswordResetRequest(User? user)
    {
        if (user?.Email is not null)
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);

            var link = urlHelper.PageLink("ResetPassword", values: new { userId = user.Id, code = code })!;

            await emailSender.SendEmailAsync(
                user.Email,
                "Password Reset",
                $"""
                Please <a href="{HtmlEncoder.Default.Encode(link)}">click here</a> to reset your password.
                If you didn't request this, ignore this email and do not click the link.
                """
            );
        }

        return new ItemResult(true,
            "If the user account exists, the email address on the account " +
            "will receive an email shortly with password reset instructions.");
    }
}
