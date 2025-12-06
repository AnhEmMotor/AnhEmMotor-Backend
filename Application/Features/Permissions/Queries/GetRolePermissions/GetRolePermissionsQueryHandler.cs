using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories.Authorization;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Queries.GetRolePermissions;

public class GetRolePermissionsQueryHandler(
    RoleManager<ApplicationRole> roleManager,
    IRolePermissionRepository rolePermissionRepository) : IRequestHandler<GetRolePermissionsQuery, List<PermissionResponse>>
{
    public async Task<List<PermissionResponse>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByNameAsync(request.RoleName) ?? throw new NotFoundException("Role not found.");
        var permissions = await rolePermissionRepository.GetPermissionNamesByRoleIdAsync(role.Id, cancellationToken);

        var permissionsWithMetadata = permissions
            .Select(p => new
            {
                Name = p,
                Metadata = PermissionsList.GetMetadata(p)
            })
            .Select(p => new PermissionResponse
            {
                ID = p.Name,
                DisplayName = p.Metadata?.DisplayName ?? p.Name,
                Description = p.Metadata?.Description
            })
            .ToList();

        return permissionsWithMetadata;
    }
}
