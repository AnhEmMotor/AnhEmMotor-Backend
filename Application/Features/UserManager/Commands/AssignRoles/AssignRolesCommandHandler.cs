using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Repositories.UserManager;
using Application.Interfaces.Services;
using Domain.Constants;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Features.UserManager.Commands.AssignRoles;

public class AssignRolesCommandHandler(
    IUserReadRepository userReadRepository,
    IUserUpdateRepository userUpdateRepository,
    IUserCreateRepository userCreateRepository,
    IUserManagerReadRepository userManagerReadRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<AssignRolesCommand, AssignRoleResponse>
{
    public async Task<AssignRoleResponse> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        foreach(string role in request.Model.RoleNames)
        {
            if(string.IsNullOrWhiteSpace(role))
            {
                throw new ValidationException(
                    [ new ValidationFailure("RoleNames", "Role names cannot contain empty or whitespace values.") ]);
            }
            if(UserStatus.IsValid(role))
            {
                throw new ValidationException([ new ValidationFailure("RoleNames", "Role names not vaild.") ]);
            }
        }

        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");

        var invalidRoles = new List<string>();
        foreach(var roleName in request.Model.RoleNames)
        {
            var roleExists = await userManagerReadRepository.RoleExistsAsync(roleName, cancellationToken).ConfigureAwait(false);
            if(!roleExists)
            {
                invalidRoles.Add(roleName);
            }
        }

        if(invalidRoles.Count > 0)
        {
            throw new ValidationException(
                [ new ValidationFailure(
                    "RoleNames",
                    $"The following roles do not exist: {string.Join(", ", invalidRoles)}") ]);
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
                    throw new ValidationException(
                        [ new ValidationFailure(
                            "RoleNames",
                            $"Cannot remove SuperRole '{roleToRemove}' from user. This is the last user with this role.") ]);
                }
            }
        }

        if(currentRoles.Count > 0)
        {
            var (removeSucceeded, removeErrors) = await userUpdateRepository.RemoveUserFromRolesAsync(user, currentRoles, cancellationToken).ConfigureAwait(false);
            if(!removeSucceeded)
            {
                var failures = new List<ValidationFailure>();
                foreach(var error in removeErrors)
                {
                    failures.Add(new ValidationFailure(string.Empty, error));
                }
                throw new ValidationException(failures);
            }
        }

        if(request.Model.RoleNames.Count > 0)
        {
            var (addSucceeded, addErrors) = await userCreateRepository.AddUserToRolesAsync(user, request.Model.RoleNames, cancellationToken).ConfigureAwait(false);
            if(!addSucceeded)
            {
                var failures = new List<ValidationFailure>();
                foreach(var error in addErrors)
                {
                    failures.Add(new ValidationFailure(string.Empty, error));
                }
                throw new ValidationException(failures);
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
