using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Application.Features.Permissions.Commands.DeleteRole;

public class DeleteRoleCommandHandler(
    IRoleReadRepository roleReadRepository,
    IRoleDeleteRepository roleDeleteRepository,
    IProtectedEntityManagerService protectedEntityManagerService) : IRequestHandler<DeleteRoleCommand, Result<RoleDeleteResponse>>
{
    public async Task<Result<RoleDeleteResponse>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.RoleName;

        var role = await roleReadRepository.GetRoleByNameAsync(roleName!).ConfigureAwait(false);
        if (role is null)
        {
            return Error.BadRequest("Role not found.");
        }

        var superRoles = protectedEntityManagerService.GetSuperRoles() ?? [];
        if(superRoles.Contains(roleName))
        {
            return Error.BadRequest("Cannot delete SuperRole.");
        }

        var usersWithRole = await roleReadRepository.GetUsersInRoleAsync(roleName!).ConfigureAwait(false);
        if(usersWithRole.Count > 0)
        {
            return Error.BadRequest($"Cannot delete role '{roleName}' because {usersWithRole.Count} user(s) have this role.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var result = await roleDeleteRepository.DeleteAsync(role).ConfigureAwait(false);
        if(!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Error.BadRequest(errors);
        }

        return new RoleDeleteResponse() { Message = $"Role '{roleName}' deleted successfully." };
    }
}
