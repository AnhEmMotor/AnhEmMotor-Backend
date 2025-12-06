using Application.ApiContracts.Permission.Responses;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Queries.GetAllRoles;

public class GetAllRolesQueryHandler(RoleManager<ApplicationRole> roleManager, IAsyncQueryableExecuter asyncExecuter) : IRequestHandler<GetAllRolesQuery, List<RoleSelectResponse>>
{
    public async Task<List<RoleSelectResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await asyncExecuter.ToListAsync(
            roleManager.Roles.Select(r => new RoleSelectResponse { ID = r.Id, Name = r.Name }),
            cancellationToken)
            .ConfigureAwait(false);

        return roles;
    }
}
