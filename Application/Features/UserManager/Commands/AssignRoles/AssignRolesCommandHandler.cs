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
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<AssignRolesCommand, Result<AssignRoleResponse>>
{
    public async Task<Result<AssignRoleResponse>> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            return Error.NotFound("User not found.");
        }

        var requestedRoles = request.Model.RoleNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // 1. Bulk Validation: Check tất cả Role có tồn tại trong hệ thống không (1 Query)
        // Repo cần hàm GetExistingRolesAsync nhận List<string>
        var existingSystemRoles = await roleReadRepository.GetRolesByNamesAsync(requestedRoles, cancellationToken)
            .ConfigureAwait(false);

        var existingRoleNames = existingSystemRoles.Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var invalidRoles = requestedRoles.Where(r => !existingRoleNames.Contains(r)).ToList();

        if (invalidRoles.Count > 0)
        {
            return Error.Validation($"The following roles do not exist: {string.Join(", ", invalidRoles)}", "RoleNames");
        }

        // 2. Calculate Delta (Diff)
        var currentUserRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken)
            .ConfigureAwait(false);

        var rolesToAdd = requestedRoles.Except(currentUserRoles, StringComparer.OrdinalIgnoreCase).ToList();
        var rolesToRemove = currentUserRoles.Except(requestedRoles, StringComparer.OrdinalIgnoreCase).ToList();

        // 3. Logic "Last Man Standing" cho Super Admin (Chỉ check trên danh sách cần xóa)
        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];

        foreach (var roleToRemove in rolesToRemove)
        {
            if (superRoles.Contains(roleToRemove))
            {
                var usersWithThisRole = await userReadRepository.GetUsersInRoleAsync(roleToRemove, cancellationToken)
                    .ConfigureAwait(false);

                // Nếu user hiện tại là người cuối cùng có quyền SuperRole này -> Chặn xóa
                if (usersWithThisRole.Count <= 1 && usersWithThisRole.Any(u => u.Id == request.UserId))
                {
                    return Error.Validation($"Cannot remove SuperRole '{roleToRemove}'. This user is the last one holding this role.", "RoleNames");
                }
            }
        }

        // 4. Execute Changes (Chỉ tác động những cái thay đổi)
        if (rolesToRemove.Count > 0)
        {
            var (removeSucceeded, removeErrors) = await userUpdateRepository.RemoveUserFromRolesAsync(user, rolesToRemove, cancellationToken)
                .ConfigureAwait(false);

            if (!removeSucceeded)
            {
                return Result<AssignRoleResponse>.Failure(removeErrors.Select(e => Error.Failure(e)).ToList());
            }
        }

        if (rolesToAdd.Count > 0)
        {
            var (addSucceeded, addErrors) = await userCreateRepository.AddUserToRolesAsync(user, rolesToAdd, cancellationToken)
                .ConfigureAwait(false);

            if (!addSucceeded)
            {
                return Result<AssignRoleResponse>.Failure(addErrors.Select(e => Error.Failure(e)).ToList());
            }
        }

        // 5. Return latest state
        // Nếu Identity tracking tốt, user.Roles đã tự update, nhưng an toàn thì fetch lại list string
        var finalRoles = await userReadRepository.GetUserRolesAsync(user, cancellationToken)
            .ConfigureAwait(false);

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