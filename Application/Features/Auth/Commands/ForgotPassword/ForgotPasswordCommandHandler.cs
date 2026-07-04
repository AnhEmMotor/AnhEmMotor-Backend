using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    ITokenManagerService tokenManagerService,
    IEmailService emailService,
    IConfiguration configuration) : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrEmpty(email))
            return Error.BadRequest("Email is required.");

        var user = await userReadRepository.FindUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null || user.DeletedAt is not null)
            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "If an account with that email exists, a reset link has been sent."
            };

        var resetToken = tokenManagerService.CreateRandomToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTimeOffset.UtcNow.AddHours(1);

        await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);

        var frontendBase = configuration["Frontend:AdminUrl"] ?? "http://localhost:5174";
        var resetLink = $"{frontendBase}/auth/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";

        var emailBody = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px;'>
<h2>Đặt lại mật khẩu AnhEmMotor</h2>
<p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấn vào liên kết bên dưới để tiếp tục:</p>
<a href='{resetLink}' style='display: inline-block; background: #2563eb; color: white; padding: 12px 24px; border-radius: 8px; text-decoration: none;'>Đặt lại mật khẩu</a>
<p>Liên kết này sẽ hết hạn sau 1 giờ. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
</body>
</html>";

        try
        {
            await emailService.SendEmailAsync(email, "Đặt lại mật khẩu — AnhEmMotor", emailBody, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // Don't expose email sending failure to prevent user enumeration
        }

        return new ForgotPasswordResponse
        {
            Success = true,
            Message = "If an account with that email exists, a reset link has been sent."
        };
    }
}
