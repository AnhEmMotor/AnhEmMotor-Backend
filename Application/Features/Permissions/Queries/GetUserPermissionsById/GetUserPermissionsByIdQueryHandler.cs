using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Permissions.Queries.GetUserPermissionsById;

public class GetUserPermissionsByIdQueryHandler(
    IUserReadRepository userReadRepository,
    IRoleReadRepository roleReadRepository) : IRequestHandler<GetUserPermissionsByIdQuery, Result<PermissionAndRoleOfUserResponse>>
{
    public async Task<Result<PermissionAndRoleOfUserResponse>> Handle(
        GetUserPermissionsByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userReadRepository.FindUserByIdAsync(request.UserId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        var userRoles = await userReadRepository.GetRolesOfUserAsync(user, cancellationToken).ConfigureAwait(false);
        var roleEntities = await roleReadRepository.GetRolesByNameAsync(userRoles, cancellationToken)
            .ConfigureAwait(false);

        var roleIds = roleEntities.Select(r => r.Id).ToList();
        var userPermissionNames = await roleReadRepository.GetPermissionsNameByRoleIdAsync(roleIds, cancellationToken)
            .ConfigureAwait(false);

        var userPermissions = userPermissionNames
            .Select(p => new { Name = p, Metadata = Domain.Constants.Permission.PermissionsList.GetMetadata(p) })
            .Select(p => p.Name)
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
