using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public class DeleteCurrentUserAccountCommandHandler(
    IUserReadRepository userReadRepository,
    IUserDeleteRepository userDeleteRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<DeleteCurrentUserAccountCommand, Result<DeleteUserByUserReponse>>
{
    public async Task<Result<DeleteUserByUserReponse>> Handle(
        DeleteCurrentUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId!);

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }


        if(user.DeletedAt is not null)
        {
            return Error.Validation("This account has already been deleted.", "DeletedAt");
        }

        var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

        if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
        {
            return Error.Validation("Protected users cannot delete their account.", "Email");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userDeleteRepository.SoftDeleteUserAsync(user, cancellationToken)
            .ConfigureAwait(false);
        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<DeleteUserByUserReponse>.Failure(validationErrors);
        }

        return new DeleteUserByUserReponse() { Message = "Your account has been deleted successfully.", };
    }
}
