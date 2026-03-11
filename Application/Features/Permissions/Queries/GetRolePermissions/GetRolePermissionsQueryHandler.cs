using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Domain.Constants.Permission;
using MediatR;

namespace Application.Features.Permissions.Queries.GetRolePermissions;

public class GetRolePermissionsQueryHandler(IRoleReadRepository rolePermissionRepository) : IRequestHandler<GetRolePermissionsQuery, Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await rolePermissionRepository.GetRolesByIdsAsync([request.RoleId], cancellationToken)
            .ConfigureAwait(false);
        var role = roles.FirstOrDefault();

        if(role is null)
        {
            return Error.NotFound("Role not found.");
        }

        var permissions = await rolePermissionRepository.GetPermissionsNameByRoleIdAsync(role.Id, cancellationToken)
                .ConfigureAwait(false) ??
            [];

        return permissions.ToList();
    }
}
