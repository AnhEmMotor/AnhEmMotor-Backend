using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Users.Commands.ChangePasswordCurrentUser;

public class ChangePasswordCurrentUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordCurrentUserCommand, Result<ChangePasswordUserByUserResponse>>
{
    public async Task<Result<ChangePasswordUserByUserResponse>> Handle(
        ChangePasswordCurrentUserCommand request,
        CancellationToken cancellationToken)
    {
        if(string.Compare(request.Model.CurrentPassword, request.Model.NewPassword) == 0)
        {
            return Error.Validation("New password can not dupplicate current password.", "newPassword");
        }

        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.Unauthorized("Invalid user token.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        var (succeeded, errors) = await userUpdateRepository.ChangePasswordAsync(
            user,
            request.Model.CurrentPassword,
            request.Model.NewPassword,
            cancellationToken)
            .ConfigureAwait(false);

        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<ChangePasswordUserByUserResponse>.Failure(validationErrors);
        }

        return new ChangePasswordUserByUserResponse() { Message = "Password changed successfully." };
    }
}
