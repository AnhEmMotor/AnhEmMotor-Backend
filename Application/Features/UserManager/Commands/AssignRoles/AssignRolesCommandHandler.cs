using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants;
using MediatR;

namespace Application.Features.UserManager.Commands.AssignRoles;

public class AssignRolesCommandHandler(
    IUserReadRepository userReadRepository,
    IRoleReadRepository roleReadRepository, 
    IUserUpdateRepository userUpdateRepository,
    IUserCreateRepository userCreateRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<AssignRolesCommand, Result<AssignRoleResponse>>
{
    public async Task<Result<AssignRoleResponse>> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        foreach(string role in request.Model.RoleNames)
        {
            if(string.IsNullOrWhiteSpace(role))
            {
                return Error.Validation("Role names cannot contain empty or whitespace values.", "RoleNames");
            }
            if(UserStatus.IsValid(role))
            {
                return Error.Validation("Role names not valid.", "RoleNames");
            }
        }

        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        var invalidRoles = new List<string>();
        foreach(var roleName in request.Model.RoleNames)
        {
            var roleExists = await roleReadRepository.IsRoleExistsAsync(roleName, cancellationToken).ConfigureAwait(false);
            if(!roleExists)
            {
                invalidRoles.Add(roleName);
            }
        }

        if(invalidRoles.Count > 0)
        {
            return Error.Validation($"The following roles do not exist: {string.Join(", ", invalidRoles)}", "RoleNames");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var currentRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);
        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];

        var rolesToRemove = currentRoles.Except(request.Model.RoleNames).ToList();
        foreach(var roleToRemove in rolesToRemove)
        {
            if(superRoles.Contains(roleToRemove))
            {
                var usersWithThisRole = await userReadRepository.GetUsersInRoleAsync(roleToRemove, cancellationToken).ConfigureAwait(false);
                if(usersWithThisRole.Count == 1 && usersWithThisRole[0].Id == request.UserId)
                {
                    return Error.Validation($"Cannot remove SuperRole '{roleToRemove}' from user. This is the last user with this role.", "RoleNames");
                }
            }
        }

        if(currentRoles.Count > 0)
        {
            var (removeSucceeded, removeErrors) = await userUpdateRepository.RemoveUserFromRolesAsync(user, currentRoles, cancellationToken).ConfigureAwait(false);
            if(!removeSucceeded)
            {
                var errors = removeErrors.Select(e => Error.Failure(e)).ToList();
                return Result<AssignRoleResponse>.Failure(errors);
            }
        }

        if(request.Model.RoleNames.Count > 0)
        {
            var (addSucceeded, addErrors) = await userCreateRepository.AddUserToRolesAsync(user, request.Model.RoleNames, cancellationToken).ConfigureAwait(false);
            if(!addSucceeded)
            {
                var errors = addErrors.Select(e => Error.Failure(e)).ToList();
                return Result<AssignRoleResponse>.Failure(errors);
            }
        }

        var updatedRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);

        return new AssignRoleResponse()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Roles = updatedRoles
        };
    }
}
