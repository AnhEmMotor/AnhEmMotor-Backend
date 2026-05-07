
namespace Application.Interfaces.Services;

public interface IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellation);
}
