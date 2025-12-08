using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Commands.DeleteRole;

public class DeleteRoleCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<DeleteRoleCommand, RoleDeleteResponse>
{
    public async Task<RoleDeleteResponse> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.RoleName;

        var role = await roleManager.FindByNameAsync(roleName).ConfigureAwait(false) ??
            throw new NotFoundException("Role not found.");

        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];
        if(superRoles.Contains(roleName))
        {
            throw new BadRequestException("Cannot delete SuperRole.");
        }

        var usersWithRole = await userManager.GetUsersInRoleAsync(roleName).ConfigureAwait(false);
        if(usersWithRole.Count > 0)
        {
            throw new BadRequestException(
                $"Cannot delete role '{roleName}' because {usersWithRole.Count} user(s) have this role.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var result = await roleManager.DeleteAsync(role).ConfigureAwait(false);
        if(!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }

        return new RoleDeleteResponse() { Message = $"Role '{roleName}' deleted successfully." };
    }
}
