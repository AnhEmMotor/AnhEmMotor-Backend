using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Role;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllRoles;

public class GetAllRolesQueryHandler(IRoleReadRepository roleReadRepository) : IRequestHandler<GetAllRolesQuery, Result<PagedResult<RoleSelectResponse>>>
{
    public async Task<Result<PagedResult<RoleSelectResponse>>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var result = await roleReadRepository.GetPagedRolesSelectAsync(request, cancellationToken).ConfigureAwait(false);
        return result;
    }
}