using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;
using Application;
using Application.Features;
using Application.Features.UserManager;
using Application.Features.UserManager.Commands;

namespace Application.Features.UserManager.Commands.ChangePasswordByManager;

public class ChangePasswordByManagerCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordByManagerCommand, Result<ChangePasswordByManagerResponse>>
{
    public async Task<Result<ChangePasswordByManagerResponse>> Handle(
        ChangePasswordByManagerCommand request,
        CancellationToken cancellationToken)
    {
        // NOTE: In UserManager context, we assume the caller has "Users.ChangePassword" permission (checked by controller).
        // If the intention is for an admin to *reset* another user's password, we don't need CurrentPassword,
        // and we shouldn't check if currentUser == targetUser.

        var user = await userReadRepository.FindUserByIdAsync(request.UserId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Use ResetPassword approach since Admin usually doesn't know user's current password.
        // Assuming this endpoint is for Admin forcing a password change.
        var (succeeded, errors) = await userUpdateRepository.ResetPasswordAsync(
            user,
            request.NewPassword!,
            cancellationToken)
            .ConfigureAwait(false);

        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<ChangePasswordByManagerResponse>.Failure(validationErrors);
        }

        // Invalidate refresh tokens upon password change
        await userUpdateRepository.ClearRefreshTokenAsync(user.Id, cancellationToken).ConfigureAwait(false);

        // Security Stamp is automatically updated by UserManager.ResetPasswordAsync/UpdateAsync usually,
        // but let's ensure token invalidation mechanisms rely on it or explicit revocation.
        // ClearRefreshTokenAsync handles the DB side for refresh tokens.

        return new ChangePasswordByManagerResponse() { Message = "Password changed successfully." };
    }
}

