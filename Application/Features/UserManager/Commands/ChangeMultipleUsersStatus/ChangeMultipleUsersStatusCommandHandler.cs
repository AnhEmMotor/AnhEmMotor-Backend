using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;

public class ChangeMultipleUsersStatusCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<ChangeMultipleUsersStatusCommand, Result<ChangeStatusMultiUserByManagerResponse>>
{
    public async Task<Result<ChangeStatusMultiUserByManagerResponse>> Handle(
        ChangeMultipleUsersStatusCommand request,
        CancellationToken cancellationToken)
    {
        var protectedUsers = protectedEntityManagerService.GetProtectedUsers() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();
        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];

        var usersToUpdate = new List<ApplicationUser>();
        var errorMessages = new List<Error>();

        foreach(var userId in request.UserIds!)
        {
            var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
            if(user is null)
            {
                errorMessages.Add(Error.Validation($"User {userId} not found.", "UserIds"));
                continue;
            }

            if(string.Compare(request.Status, UserStatus.Banned) == 0)
            {
                if(!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
                {
                    errorMessages.Add(Error.Validation($"User {user.Email} is protected and cannot be deactivated.", "UserIds"));
                    continue;
                }

                var userRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);
                var isLastActiveInSuperRole = false;

                foreach(var userRole in userRoles)
                {
                    if(superRoles.Contains(userRole))
                    {
                        var usersInRole = await userReadRepository.GetUsersInRoleAsync(userRole, cancellationToken).ConfigureAwait(false);
                        var activeUsersInRole = usersInRole.Where(u => string.Compare(u.Status, UserStatus.Active) == 0)
                            .ToList();

                        if(activeUsersInRole.Count == 1 && activeUsersInRole[0].Id == userId)
                        {
                            errorMessages.Add(Error.Validation($"User {user.Email} is protected and cannot be deactivated.", "UserIds"));
                            isLastActiveInSuperRole = true;
                            break;
                        }
                    }
                }

                if(isLastActiveInSuperRole)
                {
                    continue;
                }
            }

            usersToUpdate.Add(user);
        }

        if(errorMessages.Count > 0)
        {
            return Result<ChangeStatusMultiUserByManagerResponse>.Failure(errorMessages);
        }

        if(usersToUpdate.Count == 0)
        {
            return Error.Validation("No valid users to update.", "UserIds");
        }

        var updatedCount = 0;
        var failedUpdates = new List<Error>();
        var originalStatuses = new Dictionary<Guid, string>();

        foreach(var user in usersToUpdate)
        {
            originalStatuses[user.Id] = user.Status;
        }

        foreach(var user in usersToUpdate)
        {
            user.Status = request.Status!;
            var (succeeded, errors) = await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);

            if(!succeeded)
            {
                failedUpdates.Add(Error.Validation($"User {user.UserName}: {string.Join(", ", errors)}", "UserIds"));
            } else
            {
                updatedCount++;
            }
        }

        if(failedUpdates.Count > 0)
        {
            foreach(var user in usersToUpdate.Take(updatedCount))
            {
                user.Status = originalStatuses[user.Id];
                await userUpdateRepository.UpdateUserAsync(user, cancellationToken).ConfigureAwait(false);
            }

            return Result<ChangeStatusMultiUserByManagerResponse>.Failure(failedUpdates);
        }

        return new ChangeStatusMultiUserByManagerResponse()
        {
            Message = $"{updatedCount} user(s) status changed to {request.Status} successfully.",
        };
    }
}

