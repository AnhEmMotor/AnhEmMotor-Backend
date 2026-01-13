using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordByManagerResponse>>
{
    public async Task<Result<ChangePasswordByManagerResponse>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.CurrentUserId) || !Guid.TryParse(request.CurrentUserId, out var currentUserGuid))
        {
            return Error.Unauthorized("Invalid user token.");
        }

        if(currentUserGuid != request.UserId!.Value)
        {
            return Error.Forbidden("Invalid user token.");
        }

        var user = await userReadRepository.FindUserByIdAsync(request.UserId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        if(string.Compare(request.CurrentPassword, request.NewPassword) == 0)
        {
            return Error.Validation("New password must be different from current password.", "NewPassword");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userUpdateRepository.ChangePasswordAsync(
            user,
            request.CurrentPassword!,
            request.NewPassword!,
            cancellationToken)
            .ConfigureAwait(false);

        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<ChangePasswordByManagerResponse>.Failure(validationErrors);
        }

        return new ChangePasswordByManagerResponse() { Message = "Password changed successfully." };
    }
}

