using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;
using Application;
using Application.Features;
using Application.Features.Users;
using Application.Features.Users.Commands;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordByUserResponse>>
{
    public async Task<Result<ChangePasswordByUserResponse>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.BadRequest("Invalid user ID.");
        }

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

        if (user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }

        if (user.Status == Domain.Constants.UserStatus.Banned)
        {
            return Error.Forbidden("User account is banned.");
        }

        var isPasswordCorrect = await userReadRepository.CheckPasswordAsync(user, request.CurrentPassword!, cancellationToken).ConfigureAwait(false);
        if (!isPasswordCorrect)
        {
            return Error.Validation("CurrentPassword", "Incorrect password.");
        }

        var (succeeded, errors) = await userUpdateRepository.ChangePasswordAsync(
            user,
            request.CurrentPassword!,
            request.NewPassword!,
            cancellationToken)
            .ConfigureAwait(false);

        if (!succeeded)
        {
            if (!errors.Any())
            {
                return Error.Validation("ChangePasswordFailed", "Failed to change password.");
            }
            var validationErrors = errors.Select(e => Error.Validation("ChangePasswordError", e)).ToList();
            return Result<ChangePasswordByUserResponse>.Failure(validationErrors);
        }

        return new ChangePasswordByUserResponse() { Message = "Password changed successfully." };
    }
}

