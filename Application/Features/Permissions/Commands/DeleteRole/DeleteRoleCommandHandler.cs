using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Permissions.Commands.DeleteRole;

public class DeleteRoleCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IRequestHandler<DeleteRoleCommand, RoleDeleteResponse>
{
    public async Task<RoleDeleteResponse> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.RoleName;

        var role = await roleManager.FindByNameAsync(roleName) ?? throw new NotFoundException("Role not found.");

        // Kiểm tra SuperRoles
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];
        if (superRoles.Contains(roleName))
        {
            throw new BadRequestException("Cannot delete SuperRole.");
        }

        // Kiểm tra không có user nào có role này
        var usersWithRole = await userManager.GetUsersInRoleAsync(roleName);
        if (usersWithRole.Count > 0)
        {
            throw new BadRequestException($"Cannot delete role '{roleName}' because {usersWithRole.Count} user(s) have this role.");
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }

        return new RoleDeleteResponse()
        {
            Message = $"Role '{roleName}' deleted successfully."
        };
    }
}
