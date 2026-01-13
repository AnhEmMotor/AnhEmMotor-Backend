using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllRoles;

public class GetAllRolesQueryHandler(IRoleReadRepository roleReadRepository) : IRequestHandler<GetAllRolesQuery, Result<List<RoleSelectResponse>>>
{
    public async Task<Result<List<RoleSelectResponse>>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await roleReadRepository.GetAllRolesSelectAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }
}