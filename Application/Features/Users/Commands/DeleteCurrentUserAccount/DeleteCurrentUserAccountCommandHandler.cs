using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public class DeleteCurrentUserAccountCommandHandler(
    IUserReadRepository userReadRepository,
    IUserDeleteRepository userDeleteRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<DeleteCurrentUserAccountCommand, Result<DeleteAccountByUserReponse>>
{
    public async Task<Result<DeleteAccountByUserReponse>> Handle(
        DeleteCurrentUserAccountCommand request,
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
            return Error.Validation("DeletedAt", "This account has already been deleted.");
        }

        if (user.Status == Domain.Constants.UserStatus.Banned)
        {
            return Error.Forbidden("User account is banned.");
        }

        var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

        if (!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
        {
            return Error.Validation("Email", "Protected users cannot delete their account.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userDeleteRepository.SoftDeleteUserAsync(user, cancellationToken)
            .ConfigureAwait(false);
        if (!succeeded)
        {
            if (!errors.Any())
            {
                return Error.Validation("DeleteFailed", "Failed to delete account.");
            }
            var validationErrors = errors.Select(e => Error.Validation("DeleteError", e)).ToList();
            return Result<DeleteAccountByUserReponse>.Failure(validationErrors);
        }

        return new DeleteAccountByUserReponse() { Message = "Your account has been deleted successfully.", };
    }
}
