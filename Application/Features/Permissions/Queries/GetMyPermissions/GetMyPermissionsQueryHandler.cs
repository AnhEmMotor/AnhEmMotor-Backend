using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Domain.Constants;
using Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Application.Interfaces.Repositories.Authorization;

namespace Application.Features.Permissions.Queries.GetMyPermissions;

public class GetMyPermissionsQueryHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationRoleRepository applicationRoleRepository,
    IRolePermissionRepository rolePermissionRepository) : IRequestHandler<GetMyPermissionsQuery, PermissionAndRoleOfUserResponse>
{
    public async Task<PermissionAndRoleOfUserResponse> Handle(GetMyPermissionsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException("User not found.");
        var userRoles = await userManager.GetRolesAsync(user);
        var roleEntities = await applicationRoleRepository.GetRolesByNamesAsync(userRoles!, cancellationToken);

        var roleIds = roleEntities.Select(r => r.Id).ToList();
        var userPermissionNames = await rolePermissionRepository.GetPermissionNamesByRoleIdsAsync(roleIds, cancellationToken);

        var userPermissions = userPermissionNames
            .Select(p => new
            {
                Name = p,
                Metadata = PermissionsList.GetMetadata(p)
            })
            .Select(p => new PermissionResponse()
            {
                ID = p.Name,
                DisplayName = p.Metadata?.DisplayName ?? p.Name,
                Description = p.Metadata?.Description
            })
            .ToList();

        return new PermissionAndRoleOfUserResponse()
        {
            UserId = userId,
            Email = user.Email,
            UserName = user.UserName,
            Roles = userRoles,
            Permissions = userPermissions
        };
    }
}
