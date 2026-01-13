using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Permissions.Commands.DeleteMultipleRoles;

public class DeleteMultipleRolesCommandHandler(
    IRoleReadRepository roleReadRepository,
    IUserReadRepository userReadRepository,
    IRoleDeleteRepository roleDeleteRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<DeleteMultipleRolesCommand, Result<RoleDeleteResponse>>
{
    public async Task<Result<RoleDeleteResponse>> Handle(
        DeleteMultipleRolesCommand request,
        CancellationToken cancellationToken)
    {
        var roleNames = request.RoleNames;
        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];
        var skippedRoles = new List<string>();
        var deletedCount = 0;

        foreach(var roleName in roleNames!)
        {
            if(superRoles.Contains(roleName))
            {
                skippedRoles.Add($"{roleName} (SuperRole)");
                continue;
            }

            var role = await roleReadRepository.GetRoleByNameAsync(roleName, cancellationToken).ConfigureAwait(false);
            if(role is null)
            {
                skippedRoles.Add($"{roleName} (Not found)");
                continue;
            }

            var usersWithRole = await userReadRepository.GetUsersInRoleAsync(roleName, cancellationToken)
                .ConfigureAwait(false);
            if(usersWithRole.Count > 0)
            {
                skippedRoles.Add($"{roleName} ({usersWithRole.Count} user(s) assigned)");
                continue;
            }
        }

        if(skippedRoles.Count > 0)
        {
            return Error.BadRequest(
                $"Cannot delete some roles due to validation errors: {string.Join(',', skippedRoles)}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        foreach(var roleName in roleNames)
        {
            var role = await roleReadRepository.GetRoleByNameAsync(roleName, cancellationToken).ConfigureAwait(false);
            if(role is not null)
            {
                var result = await roleDeleteRepository.DeleteAsync(role, cancellationToken).ConfigureAwait(false);
                if(result.Succeeded)
                {
                    deletedCount++;
                }
            }
        }

        return new RoleDeleteResponse() { Message = $"Deleted {deletedCount} role(s) successfully." };
    }
}
