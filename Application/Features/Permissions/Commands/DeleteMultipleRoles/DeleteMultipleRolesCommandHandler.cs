using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Commands.DeleteMultipleRoles;

public class DeleteMultipleRolesCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
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

        foreach(var roleName in roleNames)
        {
            if(superRoles.Contains(roleName))
            {
                skippedRoles.Add($"{roleName} (SuperRole)");
                continue;
            }

            var role = await roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
            if(role is null)
            {
                skippedRoles.Add($"{roleName} (Not found)");
                continue;
            }

            var usersWithRole = await userManager.GetUsersInRoleAsync(roleName).ConfigureAwait(false);
            if(usersWithRole.Count > 0)
            {
                skippedRoles.Add($"{roleName} ({usersWithRole.Count} user(s) assigned)");
                continue;
            }
        }

        if(skippedRoles.Count > 0)
        {
                        return Error.BadRequest($"Cannot delete some roles due to validation errors: {string.Join(',', skippedRoles)}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        foreach(var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
            if(role is not null)
            {
                var result = await roleManager.DeleteAsync(role).ConfigureAwait(false);
                if(result.Succeeded)
                {
                    deletedCount++;
                }
            }
        }

        return new RoleDeleteResponse() { Message = $"Deleted {deletedCount} role(s) successfully." };
    }
}
