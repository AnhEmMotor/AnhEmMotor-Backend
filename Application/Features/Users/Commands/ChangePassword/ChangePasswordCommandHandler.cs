using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserReadRepository userReadRepository,
    ICurrentUserContext currentUserContext,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordByUserResponse>>
{
    public async Task<Result<ChangePasswordByUserResponse>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }
        if (user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }
        if (string.Compare(user.Status, UserStatus.Banned) == 0)
        {
            return Error.Forbidden("User account is banned.");
        }
        var isPasswordCorrect = await userReadRepository.CheckPasswordAsync(
            user,
            request.CurrentPassword!,
            cancellationToken)
            .ConfigureAwait(false);
        if (!isPasswordCorrect)
        {
            return Error.Validation("Incorrect password.", "CurrentPassword");
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
                return Error.Validation("Failed to change password.", "ChangePasswordFailed");
            }
            var validationErrors = errors.Select(e => Error.Validation(e, "NewPassword")).ToList();
            return Result<ChangePasswordByUserResponse>.Failure(validationErrors);
        }
        return new ChangePasswordByUserResponse() { Message = "Password changed successfully." };
    }
}

