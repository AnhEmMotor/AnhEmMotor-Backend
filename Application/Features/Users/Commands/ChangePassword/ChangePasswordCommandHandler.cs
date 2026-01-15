using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;
using Application;
using Application.Features;
using Application.Features.Users;
using Application.Features.Users.Commands;

namespace Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordByUserResponse>>
{
    public async Task<Result<ChangePasswordByUserResponse>> Handle(
        ChangePasswordCommand request,
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
            return Result<ChangePasswordByUserResponse>.Failure(validationErrors);
        }

        return new ChangePasswordByUserResponse() { Message = "Password changed successfully." };
    }
}

