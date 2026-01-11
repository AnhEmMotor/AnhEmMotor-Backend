using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<ChangeUserStatusCommand, Result<ChangeStatusUserByManagerResponse>>
{
    public async Task<Result<ChangeStatusUserByManagerResponse>> Handle(
        ChangeUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        if(!UserStatus.IsValid(request.Model.Status))
        {
            return Error.Validation("Status not vaild, please check.", "Status");
        }

        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        if(string.Compare(request.Model.Status, UserStatus.Banned) == 0)
        {
            var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
            var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();

            if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
            {
                return Error.Validation("Cannot deactivate protected user.", "Email");
            }

            var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];
            var userRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

            foreach(var userRole in userRoles)
            {
                if(superRoles.Contains(userRole))
                {
                    var usersInRole = await userReadRepository.GetUsersInRoleAsync(userRole, cancellationToken).ConfigureAwait(false);
                    var activeUsersInRole = usersInRole.Where(u => string.Compare(u.Status, UserStatus.Active) == 0)
                        .ToList();

                    if(activeUsersInRole.Count == 1 && activeUsersInRole[0].Id == request.UserId)
                    {
                        return Error.Validation($"Cannot deactivate user. This is the last active user with SuperRole '{userRole}'.", "Status");
                    }
                }
            }
        }

        user.Status = request.Model.Status;
        var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
        if(!succeeded)
        {
            var validationErrors = errors.Select(e => Error.Validation(e)).ToList();
            return Result<ChangeStatusUserByManagerResponse>.Failure(validationErrors);
        }

        return new ChangeStatusUserByManagerResponse()
        {
            Message = $"User status changed to {request.Model.Status} successfully.",
        };
    }
}
