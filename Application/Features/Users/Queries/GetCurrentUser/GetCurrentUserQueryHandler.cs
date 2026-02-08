using Application.ApiContracts.Permission.Responses;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    IUserReadRepository userReadRepository,
    IRoleReadRepository roleReadRepository) : IRequestHandler<GetCurrentUserQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Error.BadRequest("Invalid user ID.");
        }

        var user = await userReadRepository.FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if(user is null)
        {
            return Error.NotFound("User not found.");
        }

        if(user.DeletedAt is not null)
        {
            return Error.Forbidden("User account is deleted.");
        }

        // Allow banned users to retrieve their profile so frontend can show "Banned" status
        // if(string.Compare(user.Status, Domain.Constants.UserStatus.Banned) == 0)
        // {
        //    return Error.Forbidden("User account is banned.");
        // }

        var userRoles = await userReadRepository.GetRolesOfUserAsync(user, cancellationToken).ConfigureAwait(false);
        var roleEntities = await roleReadRepository.GetRolesByNameAsync(userRoles, cancellationToken)
            .ConfigureAwait(false);

        var roleIds = roleEntities.Select(r => r.Id).ToList();
        var userPermissionNames = await roleReadRepository.GetPermissionsNameByRoleIdAsync(roleIds, cancellationToken)
                .ConfigureAwait(false) ??
            new List<string>();

        List<PermissionResponse>? userPermissions = null;
        if(userPermissionNames.Count > 0)
        {
            userPermissions = userPermissionNames
                .Select(p => new { Name = p, Metadata = Domain.Constants.Permission.PermissionsList.GetMetadata(p) })
                .Select(
                    p => new PermissionResponse()
                    {
                        ID = p.Name,
                        DisplayName = p.Metadata?.DisplayName ?? p.Name,
                        Description = p.Metadata?.Description
                    })
                .ToList();
        }

        return new UserResponse()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Gender = user.Gender,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status,
            Permissions = userPermissions
        };
    }
}
