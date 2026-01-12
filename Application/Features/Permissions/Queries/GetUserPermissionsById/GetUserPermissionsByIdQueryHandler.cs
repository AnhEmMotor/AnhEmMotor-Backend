using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Queries.GetUserPermissionsById;

public class GetUserPermissionsByIdQueryHandler(
    UserManager<ApplicationUser> userManager,
    IRoleReadRepository roleReadRepository) : IRequestHandler<GetUserPermissionsByIdQuery, Result<PermissionAndRoleOfUserResponse>>
{
    public async Task<Result<PermissionAndRoleOfUserResponse>> Handle(
        GetUserPermissionsByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User not found.");
        }

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
            UserId = request.UserId,
            Email = user.Email,
            UserName = user.UserName,
            Roles = userRoles,
            Permissions = userPermissions
        };
    }
}
