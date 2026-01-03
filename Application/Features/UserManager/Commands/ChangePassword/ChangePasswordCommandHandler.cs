using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordCommand, ChangePasswordByManagerResponse>
{
    public async Task<ChangePasswordByManagerResponse> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.CurrentUserId) || !Guid.TryParse(request.CurrentUserId, out var currentUserGuid))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        if(currentUserGuid != request.UserId)
        {
            throw new ForbiddenException("Invalid user token.");
        }

        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

        if(string.Compare(request.Model.CurrentPassword, request.Model.NewPassword) == 0)
        {
            throw new ValidationException(
                [ new ValidationFailure("NewPassword", "New password must be different from current password.") ]);
        }

        cancellationToken.ThrowIfCancellationRequested();

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

        return new ChangePasswordByManagerResponse() { Message = "Password changed successfully." };
    }
}
