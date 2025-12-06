using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Permissions.Commands.DeleteMultipleRoles;

public class DeleteMultipleRolesCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : IRequestHandler<DeleteMultipleRolesCommand, RoleDeleteResponse>
{
    public async Task<RoleDeleteResponse> Handle(DeleteMultipleRolesCommand request, CancellationToken cancellationToken)
    {
        var roleNames = request.RoleNames;
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];
        var skippedRoles = new List<string>();
        var deletedCount = 0;

        // Kiểm tra trước khi xóa
        foreach (var roleName in roleNames)
        {
            // Kiểm tra SuperRoles
            if (superRoles.Contains(roleName))
            {
                skippedRoles.Add($"{roleName} (SuperRole)");
                continue;
            }

            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                skippedRoles.Add($"{roleName} (Not found)");
                continue;
            }

            // Kiểm tra có user nào có role này không
            var usersWithRole = await userManager.GetUsersInRoleAsync(roleName);
            if (usersWithRole.Count > 0)
            {
                skippedRoles.Add($"{roleName} ({usersWithRole.Count} user(s) assigned)");
                continue;
            }
        }

        // Nếu có skipped roles, báo lỗi
        if (skippedRoles.Count > 0)
        {
            throw new BadRequestException($"Cannot delete some roles due to validation errors: {string.Join(',', skippedRoles)}");
        }

        // Xóa tất cả roles
        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    deletedCount++;
                }
            }
        }

        return new RoleDeleteResponse()
        {
            Message = $"Deleted {deletedCount} role(s) successfully."
        };
    }
}
