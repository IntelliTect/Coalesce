
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Coalesce.Starter.Vue.Data.Communication;

public class SendGridEmailOptions
{
    public string? ApiKey { get; set; }

    public string? SenderEmail { get; set; }
}

public class SendGridEmailService(
    IHostEnvironment env,
    IOptionsMonitor<SendGridEmailOptions> config,
    ILogger<SendGridEmailService> logger
) : IEmailService
{
    public async Task<ItemResult> SendEmailAsync(string to, string subject, string htmlMessage)
    {
        if (!env.IsProduction())
        {
            return await new NoOpEmailService(env)
                .SendEmailAsync(to, subject, htmlMessage);
        }

        var apiKey = config.CurrentValue.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "Email service not configured.";
        }

        var client = new SendGridClient(apiKey);
        SendGridMessage message = new()
        {
            From = new EmailAddress(config.CurrentValue.SenderEmail),
            Subject = subject,
            HtmlContent = htmlMessage
        };
        message.AddTo(new EmailAddress(to));

        try
        {
            Response? response = await client.SendEmailAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Error sending email '{subject}' to {to}: Status {status}, error {error}", 
                    subject, 
                    to, 
                    response.StatusCode, 
                    await response.Body.ReadAsStringAsync());

                return new ItemResult(false, $"Unable to send email.");
            }

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
