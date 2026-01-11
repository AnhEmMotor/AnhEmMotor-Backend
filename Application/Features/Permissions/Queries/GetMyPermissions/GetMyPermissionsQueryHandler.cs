using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Queries.GetMyPermissions;

public class GetMyPermissionsQueryHandler(
    UserManager<ApplicationUser> userManager,
    IRoleReadRepository roleReadRepository) : IRequestHandler<GetMyPermissionsQuery, Result<PermissionAndRoleOfUserResponse>>
{
    public async Task<Result<PermissionAndRoleOfUserResponse>> Handle(
        GetMyPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.Unauthorized("Invalid user token.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false) ?? 
            return Error.NotFound("User not found.");
        var userRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        var roleEntities = await roleReadRepository.GetRolesByNamesAsync(userRoles!, cancellationToken)
            .ConfigureAwait(false);

        var roleIds = roleEntities.Select(r => r.Id).ToList();
        var userPermissionNames = await roleReadRepository.GetPermissionNamesByRoleIdsAsync(roleIds, cancellationToken)
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
            UserId = userId,
            Email = user.Email,
            UserName = user.UserName,
            Roles = userRoles,
            Permissions = userPermissions
        };
    }
}
