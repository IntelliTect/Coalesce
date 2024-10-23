using Microsoft.Extensions.Hosting;

namespace Coalesce.Starter.Vue.Data.Communication;

public class NoOpEmailService(
    IHostEnvironment env
) : IEmailService
{
    public Task<ItemResult> SendEmailAsync(string to, string subject, string htmlMessage)
    {
        if (env.IsProduction())
        {
            throw new NotImplementedException("Email sending has not been implemented.");
        }

        // When email sending has not been implemented, dump the email content into the result message
        // so that essential functions during initial development (e.g. account setup links)
        // can still be used.

        return Task.FromResult(new ItemResult(true, 
            $"DEVEOPMENT ONLY: Email sending is not configured, or is disabled by configuration. " +
            $"The following content would have been sent to {to}:\n\n{htmlMessage}\n\n"));
    }
}