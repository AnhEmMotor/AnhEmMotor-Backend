using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    public async Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        var token = request.Token?.Trim();
        var newPassword = request.NewPassword;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            return Error.BadRequest("All fields are required.");

        var user = await userReadRepository.FindUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null || user.DeletedAt is not null)
            return Error.NotFound("User not found.");

        if (string.IsNullOrEmpty(user.PasswordResetToken) ||
            string.Compare(user.PasswordResetToken, token, StringComparison.Ordinal) != 0 ||
            user.PasswordResetTokenExpiry is null ||
            user.PasswordResetTokenExpiry < DateTimeOffset.UtcNow)
        {
            return Error.BadRequest("Invalid or expired reset token.");
        }

        var passwordHasher = new PasswordHasher<ApplicationUser>();
        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);

        return new ResetPasswordResponse
        {
            Success = true,
            Message = "Password has been reset successfully."
        };
    }
}
