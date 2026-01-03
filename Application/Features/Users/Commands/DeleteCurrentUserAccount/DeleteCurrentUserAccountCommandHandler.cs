using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public class DeleteCurrentUserAccountCommandHandler(
    IUserReadRepository userReadRepository,
    IUserDeleteRepository userDeleteRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<DeleteCurrentUserAccountCommand, DeleteUserByUserReponse>
{
    public async Task<DeleteUserByUserReponse> Handle(
        DeleteCurrentUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");


        if(user.DeletedAt is not null)
        {
            throw new ValidationException(
                [ new ValidationFailure("DeletedAt", "This account has already been deleted.") ]);
        }

        var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

        if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
        {
            throw new ValidationException(
                [ new ValidationFailure("Email", "Protected users cannot delete their account.") ]);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userDeleteRepository.SoftDeleteUserAsync(user, cancellationToken).ConfigureAwait(false);
        if(!succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach(var error in errors)
            {
                failures.Add(new ValidationFailure(string.Empty, error));
            }
            throw new ValidationException(failures);
        }

        return new DeleteUserByUserReponse() { Message = "Your account has been deleted successfully.", };
    }
}
