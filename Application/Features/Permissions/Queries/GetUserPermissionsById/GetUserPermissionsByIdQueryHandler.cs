using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.Authorization;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Queries.GetUserPermissionsById;

public class GetUserPermissionsByIdQueryHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationRoleRepository applicationRoleRepository,
    IRolePermissionRepository rolePermissionRepository) : IRequestHandler<GetUserPermissionsByIdQuery, PermissionAndRoleOfUserResponse>
{
    public async Task<PermissionAndRoleOfUserResponse> Handle(
        GetUserPermissionsByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false) ??
            throw new NotFoundException("User not found.");
        var userRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var roleEntities = await applicationRoleRepository.GetRolesByNamesAsync(userRoles!, cancellationToken)
            .ConfigureAwait(false);

        var roleIds = roleEntities.Select(r => r.Id).ToList();
        var userPermissionNames = await rolePermissionRepository.GetPermissionNamesByRoleIdsAsync(
            roleIds,
            cancellationToken)
            .ConfigureAwait(false);

        var userPermissions = userPermissionNames
            .Select(p => new { Name = p, Metadata = Domain.Constants.Permission.PermissionsList.GetMetadata(p) })
            .Select(
                p => new PermissionResponse()
                {
                    ID = p.Name,
                    DisplayName = p.Metadata?.DisplayName ?? p.Name,
                    Description = p.Metadata?.Description
                })
            .ToList();

        return new PermissionAndRoleOfUserResponse()
        {
            UserId = request.UserId,
            Email = user.Email,
            UserName = user.UserName,
            Roles = userRoles,
            Permissions = userPermissions
        };
    }
}
