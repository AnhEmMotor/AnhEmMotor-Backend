using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangePasswordByManager;

public class ChangePasswordByManagerCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository) : IRequestHandler<ChangePasswordByManagerCommand, Result<ChangePasswordByManagerResponse>>
{
    public async Task<Result<ChangePasswordByManagerResponse>> Handle(
        ChangePasswordByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var (succeeded, errors) = await userUpdateRepository.ResetPasswordAsync(
            user,
            request.NewPassword!,
            cancellationToken)
            .ConfigureAwait(false);

        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<ChangePasswordByManagerResponse>.Failure(validationErrors);
        }

        await userUpdateRepository.ClearRefreshTokenAsync(user.Id, cancellationToken).ConfigureAwait(false);

        return new ChangePasswordByManagerResponse() { Message = "Password changed successfully." };
    }
}

