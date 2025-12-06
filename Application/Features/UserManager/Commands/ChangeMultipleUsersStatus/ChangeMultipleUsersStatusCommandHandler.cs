using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Domain.Constants;
using Domain.Entities;
using Domain.Helpers;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;

public class ChangeMultipleUsersStatusCommandHandler(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IRequestHandler<ChangeMultipleUsersStatusCommand, ChangeStatusMultiUserByManagerResponse>
{
    public async Task<ChangeStatusMultiUserByManagerResponse> Handle(ChangeMultipleUsersStatusCommand request, CancellationToken cancellationToken)
    {
        if (!UserStatus.IsValid(request.Model.Status))
        {
            throw new ValidationException([new ValidationFailure("Status", "Status not vaild, please check.")]);
        }
        var protectedUsers = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers")
                .Get<List<string>>() ?? [];
        var protectedEmails = protectedUsers.Select(entry => entry.Split(':')[0].Trim()).ToList();
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];

        var usersToUpdate = new List<ApplicationUser>();
        var errorMessages = new List<ValidationFailure>();

        foreach (var userId in request.Model.UserIds)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                errorMessages.Add(new ValidationFailure("UserIds", $"User {userId} not found."));
                continue;
            }

            if (request.Model.Status == UserStatus.Inactive)
            {
                if (!string.IsNullOrEmpty(user.Email) && protectedEmails.Contains(user.Email))
                {
                    errorMessages.Add(new ValidationFailure("UserIds", $"User {user.Email} is protected and cannot be deactivated."));
                    continue;
                }

                var userRoles = await userManager.GetRolesAsync(user);
                var isLastActiveInSuperRole = false;

                foreach (var userRole in userRoles)
                {
                    if (superRoles.Contains(userRole))
                    {
                        var usersInRole = await userManager.GetUsersInRoleAsync(userRole);
                        var activeUsersInRole = usersInRole.Where(u => u.Status == UserStatus.Active).ToList();

                        if (activeUsersInRole.Count == 1 && activeUsersInRole[0].Id == userId)
                        {
                            errorMessages.Add(new ValidationFailure("UserIds", $"User {user.Email} is protected and cannot be deactivated."));
                            isLastActiveInSuperRole = true;
                            break;
                        }
                    }
                }

                if (isLastActiveInSuperRole)
                {
                    continue;
                }
            }

            usersToUpdate.Add(user);
        }

        if (errorMessages.Count > 0)
        {
            throw new ValidationException(errorMessages);
        }

        if (usersToUpdate.Count == 0)
        {
            throw new ValidationException([new ValidationFailure("UserIds", "No valid users to update.")]);
        }

        var updatedCount = 0;
        var failedUpdates = new List<ValidationFailure>();
        var originalStatuses = new Dictionary<Guid, string>();

        foreach (var user in usersToUpdate)
        {
            originalStatuses[user.Id] = user.Status;
        }

        foreach (var user in usersToUpdate)
        {
            user.Status = request.Model.Status;
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                failedUpdates.Add(new ValidationFailure("UserIds", $"User {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}"));
            }
            else
            {
                updatedCount++;
            }
        }

        if (failedUpdates.Count > 0)
        {
            foreach (var user in usersToUpdate.Take(updatedCount))
            {
                user.Status = originalStatuses[user.Id];
                await userManager.UpdateAsync(user);
            }

            throw new ValidationException(failedUpdates);
        }

        return new ChangeStatusMultiUserByManagerResponse()
        {
            Message = $"{updatedCount} user(s) status changed to {request.Model.Status} successfully.",
        };
    }
}
