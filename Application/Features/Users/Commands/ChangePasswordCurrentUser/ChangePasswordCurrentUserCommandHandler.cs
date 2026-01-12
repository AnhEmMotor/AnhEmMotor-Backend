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
        var userId = Guid.Parse(request.UserId!);

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        var (succeeded, errors) = await userUpdateRepository.ChangePasswordAsync(
            user,
            request.CurrentPassword!,
            request.NewPassword!,
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

