using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Domain.Constants.Permission;
using MediatR;

namespace Application.Features.Permissions.Queries.GetRolePermissions;

public class GetRolePermissionsQueryHandler(IRoleReadRepository rolePermissionRepository) : IRequestHandler<GetRolePermissionsQuery, Result<List<PermissionResponse>>>
{
    public async Task<Result<List<PermissionResponse>>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var role = await rolePermissionRepository.GetRoleByNameAsync(request.RoleName!, cancellationToken).ConfigureAwait(false);
        if(role is null)
        {
            return Error.NotFound("Role not found.");
        }

        var permissions = await rolePermissionRepository.GetPermissionsNameByRoleIdAsync(role.Id, cancellationToken)
            .ConfigureAwait(false);

        var permissionsWithMetadata = permissions
            .Select(p => new { Name = p, Metadata = PermissionsList.GetMetadata(p) })
            .Select(
                p => new PermissionResponse
                {
                    ID = p.Name,
                    DisplayName = p.Metadata?.DisplayName ?? p.Name,
                    Description = p.Metadata?.Description
                })
            .ToList();

        return permissionsWithMetadata;
    }
}
