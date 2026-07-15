using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.UserManager.Commands.DeleteUserByManager;

public sealed class DeleteUserByManagerCommandHandler(
    IUserReadRepository userReadRepository,
    IUserDeleteRepository userDeleteRepository,
    IProtectedEntityManagerService protectedEntityManagerService
) : IRequestHandler<DeleteUserByManagerCommand, Result>
{
    public async Task<Result> Handle(
        DeleteUserByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository
            .FindUserByIdAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            return Result.Failure("User not found.");

        if (user.DeletedAt is not null)
            return Result.Failure("This account has already been deleted.");

        if (string.Compare(user.Status, UserStatus.Banned) == 0)
            return Result.Failure("Cannot delete a banned user account.");

        var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
        var protectedEmails = protectedUsers
            .Select(entry => entry.Split(':')[0].Trim())
            .ToList();

        if (!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
            return Result.Failure("Cannot delete a protected user account.");

        var (succeeded, errors) = await userDeleteRepository
            .SoftDeleteUserAsync(user, cancellationToken)
            .ConfigureAwait(false);

        if (!succeeded)
        {
            var errorList = errors?.Any() == true
                ? errors.Select(e => $"DeleteError: {e}")
                : ["Failed to delete user."];
            return Result.Failure(string.Join("; ", errorList));
        }

        return Result.Success();
    }
}
