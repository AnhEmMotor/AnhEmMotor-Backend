using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Permission;
using MediatR;

namespace Application.Features.Contacts.Queries.GetAssignableUsers;

public class GetAssignableUsersQueryHandler(IPermissionReadRepository permissionReadRepository)
    : IRequestHandler<GetAssignableUsersQuery, Result<List<UserDTOForOutputResponse>>>
{
    public async Task<Result<List<UserDTOForOutputResponse>>> Handle(
        GetAssignableUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await permissionReadRepository
            .GetUsersWithPermissionAsync(
                Domain.Constants.Permission.Permissions.Marketing.ContactManagement.Assign,
                cancellationToken)
            .ConfigureAwait(false);
        var response = users.Select(u => new UserDTOForOutputResponse
        {
            Id = u.Id,
            FullName = u.FullName ?? string.Empty,
            Email = u.Email ?? string.Empty,
            PhoneNumber = u.PhoneNumber ?? string.Empty,
        }).ToList();
        return Result<List<UserDTOForOutputResponse>>.Success(response);
    }
}
