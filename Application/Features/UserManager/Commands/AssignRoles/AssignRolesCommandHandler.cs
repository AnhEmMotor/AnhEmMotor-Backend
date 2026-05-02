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
    IUserStreamService userStreamService) : IRequestHandler<AssignRolesCommand, Result<UserDTOForManagerResponse>>
{
    public async Task<Result<UserDTOForManagerResponse>> Handle(
        AssignRolesCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return Error.NotFound("User not found.");
        }
        var requestedRoleIds = request.RoleIds!.Distinct().ToList();
        var existingSystemRoles = await roleReadRepository.GetRolesByIdsAsync(requestedRoleIds, cancellationToken)
            .ConfigureAwait(false);
        var existingRoleIds = existingSystemRoles.Select(r => r.Id).ToHashSet();
        var invalidRoleIds = requestedRoleIds.Where(id => !existingRoleIds.Contains(id)).ToList();
        if (invalidRoleIds.Count > 0)
        {
            return Error.Validation(
                $"The following role IDs do not exist: {string.Join(", ", invalidRoleIds)}",
                "RoleIds");
        }
        var requestedRoleNames = existingSystemRoles.Select(r => r.Name!).ToList();
        var currentUserRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken).ConfigureAwait(false);
        var rolesToAdd = requestedRoleNames.Except(currentUserRoles, StringComparer.OrdinalIgnoreCase).ToList();
        var rolesToRemove = currentUserRoles.Except(requestedRoleNames, StringComparer.OrdinalIgnoreCase).ToList();
        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];
        foreach (var roleToRemove in rolesToRemove)
        {
            if (superRoles.Contains(roleToRemove))
            {
                var usersWithThisRole = await userReadRepository.GetUsersInRoleAsync(roleToRemove, cancellationToken)
                    .ConfigureAwait(false);
                if (usersWithThisRole.Count <= 1 && usersWithThisRole.Any(u => u.Id == request.UserId))
                {
                    return Error.Validation(
                        $"Cannot remove SuperRole '{roleToRemove}'. This user is the last one holding this role.",
                        "RoleIds");
                }
            }
        }
        bool hasChanged = false;
        if (rolesToRemove.Count > 0)
        {
            var (removeSucceeded, removeErrors) = await userUpdateRepository.RemoveUserFromRolesAsync(
                user,
                rolesToRemove,
                cancellationToken)
                .ConfigureAwait(false);
            if (!removeSucceeded)
            {
                return Result<UserDTOForManagerResponse>.Failure([.. removeErrors.Select(e => Error.Failure(e))]);
            }
            hasChanged = true;
        }
        if (rolesToAdd.Count > 0)
        {
            var (addSucceeded, addErrors) = await userCreateRepository.AddUserToRolesAsync(
                user,
                rolesToAdd,
                cancellationToken)
                .ConfigureAwait(false);
            if (!addSucceeded)
            {
                return Result<UserDTOForManagerResponse>.Failure([.. addErrors.Select(e => Error.Failure(e))]);
            }
            hasChanged = true;
        }
        if (hasChanged)
        {
            userStreamService.NotifyUserUpdate(user.Id);
        }
        var finalRoleIds = await userReadRepository.GetUserRoleIdsAsync(user, cancellationToken).ConfigureAwait(false);
        return new UserDTOForManagerResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Gender = user.Gender,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            Status = user.Status,
            AvatarUrl = user.AvatarUrl,
            DateOfBirth = user.DateOfBirth,
            DeletedAt = user.DeletedAt,
            Roles = finalRoleIds
        };
    }
}
