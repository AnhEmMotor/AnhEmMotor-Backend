using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
