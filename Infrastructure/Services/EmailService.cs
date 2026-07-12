using Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
private readonly SmtpSettings _settings;
private readonly ILogger _logger;

public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
{
_settings = configuration.GetSection("Smtp").Get<SmtpSettings>() ?? new SmtpSettings();
_logger = logger;
}

public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
{
cancellationToken.ThrowIfCancellationRequested();
if (string.IsNullOrWhiteSpace(_settings.Host))
{
return;
}

var message = new MimeMessage();
message.From.Add(new MailboxAddress(_settings.FromName ?? "AnhEmMotor", _settings.FromEmail ?? "noreply@anhemmotor.com"));
message.To.Add(MailboxAddress.Parse(to));
message.Subject = subject;
message.Body = new TextPart(TextFormat.Html) { Text = body };

using var client = new SmtpClient();
var socketOption = _settings.EnableSsl.GetValueOrDefault(true)
? SecureSocketOptions.SslOnConnect
: SecureSocketOptions.StartTls;

try
{
await client.ConnectAsync(_settings.Host, _settings.Port, socketOption, cancellationToken);
if (!string.IsNullOrWhiteSpace(_settings.Username))
{
await client.AuthenticateAsync(_settings.Username, _settings.Password ?? string.Empty, cancellationToken);
}
await client.SendAsync(message, cancellationToken);
}
catch (Exception)
{
throw;
}
finally
{
if (client.IsConnected)
await client.DisconnectAsync(true, cancellationToken);
}
}
}

public class SmtpSettings
{
public string Host { get; set; } = string.Empty;
public int Port { get; set; } = 587;
public string? Username { get; set; }
public string? Password { get; set; }
public string? FromEmail { get; set; }
public string? FromName { get; set; }
public bool? EnableSsl { get; set; } = true;
}
