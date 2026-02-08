using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.UserManager.Commands.AssignRoles;

public sealed class AssignRolesCommandHandler(
    IUserReadRepository userReadRepository,
    IRoleReadRepository roleReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IUserCreateRepository userCreateRepository,
    IProtectedEntityManagerService protectedEntityManagerService,
    IUserStreamService userStreamService) : IRequestHandler<AssignRolesCommand, Result<AssignRoleResponse>>
{
    public async Task<Result<AssignRoleResponse>> Handle(
        AssignRolesCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        if(user == null)
        {
            return Error.NotFound("User not found.");
        }

        var requestedRoles = request.RoleNames!.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var existingSystemRoles = await roleReadRepository.GetRolesByNameAsync(requestedRoles, cancellationToken)
            .ConfigureAwait(false);

        var existingRoleNames = existingSystemRoles.Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var invalidRoles = requestedRoles.Where(r => !existingRoleNames.Contains(r)).ToList();

        if(invalidRoles.Count > 0)
        {
            return Error.Validation($"The following roles do not exist: {string.Join(", ", invalidRoles)}", "RoleNames");
        }

        var currentUserRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

        var rolesToAdd = requestedRoles.Except(currentUserRoles, StringComparer.OrdinalIgnoreCase).ToList();
        var rolesToRemove = currentUserRoles.Except(requestedRoles, StringComparer.OrdinalIgnoreCase).ToList();

        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];

        foreach(var roleToRemove in rolesToRemove)
        {
            if(superRoles.Contains(roleToRemove))
            {
                var usersWithThisRole = await userReadRepository.GetUsersInRoleAsync(roleToRemove, cancellationToken)
                    .ConfigureAwait(false);

                if(usersWithThisRole.Count <= 1 && usersWithThisRole.Any(u => u.Id == request.UserId))
                {
                    return Error.Validation(
                        $"Cannot remove SuperRole '{roleToRemove}'. This user is the last one holding this role.",
                        "RoleNames");
                }
            }
        }

        bool hasChanged = false;

        if(rolesToRemove.Count > 0)
        {
            var (removeSucceeded, removeErrors) = await userUpdateRepository.RemoveUserFromRolesAsync(
                user,
                rolesToRemove,
                cancellationToken)
                .ConfigureAwait(false);

            if(!removeSucceeded)
            {
                return Result<AssignRoleResponse>.Failure([ .. removeErrors.Select(e => Error.Failure(e)) ]);
            }
            hasChanged = true;
        }

        if(rolesToAdd.Count > 0)
        {
            var (addSucceeded, addErrors) = await userCreateRepository.AddUserToRolesAsync(
                user,
                rolesToAdd,
                cancellationToken)
                .ConfigureAwait(false);

            if(!addSucceeded)
            {
                return Result<AssignRoleResponse>.Failure([ .. addErrors.Select(e => Error.Failure(e)) ]);
            }
            hasChanged = true;
        }

        if(hasChanged)
        {
            // Notify user about role/permission updates
            userStreamService.NotifyUserUpdate(user.Id);
        }

        var finalRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

        return new AssignRoleResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Roles = finalRoles
        };
    }
}
