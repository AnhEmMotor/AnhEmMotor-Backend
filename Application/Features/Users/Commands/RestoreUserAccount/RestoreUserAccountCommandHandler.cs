using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using MediatR;

namespace Application.Features.Users.Commands.RestoreUserAccount;

public class RestoreUserAccountCommandHandler(
    IUserReadRepository userReadRepository,
    IUserDeleteRepository userDeleteRepository) : IRequestHandler<RestoreUserAccountCommand, Result<RestoreUserResponse>>
{
    public async Task<Result<RestoreUserResponse>> Handle(
        RestoreUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        if(user.DeletedAt is null)
        {
            return Error.Validation("User account is not deleted.", "DeletedAt");
        }

        if(string.Compare(user.Status, UserStatus.Active) != 0)
        {
            return Error.Validation($"Cannot restore user with status '{user.Status}'. User status must be Active.", "Status");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userDeleteRepository.RestoreUserAsync(user, cancellationToken).ConfigureAwait(false);
        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<RestoreUserResponse>.Failure(validationErrors);
        }

        return new RestoreUserResponse() { Message = "User account has been restored successfully.", };
    }
}
