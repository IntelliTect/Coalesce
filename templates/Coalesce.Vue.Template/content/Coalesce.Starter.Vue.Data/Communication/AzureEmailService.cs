
using Azure.Communication.Email;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Coalesce.Starter.Vue.Data.Communication;

public class AzureEmailService(
    TokenCredential credential,
    IHostEnvironment env,
    IOptionsMonitor<AzureEmailOptions> config,
    ILogger<AzureEmailService> logger
) : IEmailService
{
    public async Task<ItemResult> SendEmailAsync(string to, string subject, string htmlMessage)
    {
        if (!env.IsProduction())
        {
            return await new NoOpEmailService(env)
                .SendEmailAsync(to, subject, htmlMessage);
        }

        var endpoint = config.CurrentValue.Endpoint;
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return "Email service not configured.";
        }

        // Note: Users and applications will need either the Contributor default azure role,
        // or to create a custom role per https://github.com/MicrosoftDocs/azure-docs/issues/109461#issuecomment-1642442691

        var emailClient = new EmailClient(new Uri(endpoint), credential);

        try
        {
            var result = await emailClient.SendAsync(
                senderAddress: config.CurrentValue.SenderEmail,
                recipientAddress: to,
                subject: subject,
                htmlContent: htmlMessage,
                wait: Azure.WaitUntil.Completed
            );

            logger.LogInformation("Sent email '{subject}' to {to}", subject, to);
            return new ItemResult(true, $"An email was sent to {to}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email '{subject}' to {to}", subject, to);
            return new ItemResult(false, $"Unable to send email.");
        }
    }
}
