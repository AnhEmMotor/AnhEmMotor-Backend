using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.Users.Commands.RestoreUserAccount;

public class RestoreUserAccountCommandHandler(
    IUserReadRepository userReadRepository,
    IUserDeleteRepository userDeleteRepository) : IRequestHandler<RestoreUserAccountCommand, RestoreUserResponse>
{
    public async Task<RestoreUserResponse> Handle(
        RestoreUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");
        if(user.DeletedAt is null)
        {
            throw new ValidationException([ new ValidationFailure("DeletedAt", "User account is not deleted.") ]);
        }

        if(string.Compare(user.Status, UserStatus.Active) != 0)
        {
            throw new ValidationException(
                [ new ValidationFailure(
                    "Status",
                    $"Cannot restore user with status '{user.Status}'. User status must be Active.") ]);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userDeleteRepository.RestoreUserAsync(user, cancellationToken).ConfigureAwait(false);
        if(!succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach(var error in errors)
            {
                failures.Add(new ValidationFailure(string.Empty, error));
            }
            throw new ValidationException(failures);
        }

        return new RestoreUserResponse() { Message = "User account has been restored successfully.", };
    }
}
