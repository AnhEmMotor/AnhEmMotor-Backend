using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Permissions.Queries.GetMyPermissions;

public class GetMyPermissionsQueryHandler(
    IRoleReadRepository roleReadRepository,
    IUserReadRepository userReadRepository) : IRequestHandler<GetMyPermissionsQuery, Result<PermissionAndRoleOfUserResponse>>
{
    public async Task<Result<PermissionAndRoleOfUserResponse>> Handle(
        GetMyPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId!);
        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }
        var userRoles = await userReadRepository.GetRolesOfUserAsync(user, cancellationToken).ConfigureAwait(false);
        var roleEntities = await roleReadRepository.GetRolesByNameAsync(userRoles, cancellationToken)
            .ConfigureAwait(false);

        var roleIds = roleEntities.Select(r => r.Id).ToList();
        var userPermissionNames = await roleReadRepository.GetPermissionsNameByRoleIdAsync(roleIds, cancellationToken)
                .ConfigureAwait(false) ??
            new List<string>();

        var userPermissions = userPermissionNames.ToList();

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
