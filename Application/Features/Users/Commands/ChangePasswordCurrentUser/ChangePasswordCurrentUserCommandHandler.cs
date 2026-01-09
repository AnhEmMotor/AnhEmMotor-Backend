using Application.ApiContracts.User.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.Users.Commands.ChangePasswordCurrentUser;

public class ChangePasswordCurrentUserCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordCurrentUserCommand, ChangePasswordUserByUserResponse>
{
    public async Task<ChangePasswordUserByUserResponse> Handle(
        ChangePasswordCurrentUserCommand request,
        CancellationToken cancellationToken)
    {
        if(string.Compare(request.Model.CurrentPassword, request.Model.NewPassword) == 0)
        {
            throw new ValidationException(
                [ new ValidationFailure("newPassword", "New password can not dupplicate current password.") ]);
        }

        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

        var (succeeded, errors) = await userUpdateRepository.ChangePasswordAsync(
            user,
            request.Model.CurrentPassword,
            request.Model.NewPassword,
            cancellationToken)
            .ConfigureAwait(false);

        if(!succeeded)
        {
            var failures = new List<ValidationFailure>();
            foreach(var error in errors)
            {
                failures.Add(new ValidationFailure(string.Empty, error));
            }
            throw new ValidationException(failures);
        }

        return new ChangePasswordUserByUserResponse() { Message = "Password changed successfully." };
    }
}
