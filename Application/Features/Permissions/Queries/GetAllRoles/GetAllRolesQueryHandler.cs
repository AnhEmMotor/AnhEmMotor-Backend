using Application.ApiContracts.Permission.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Queries.GetAllRoles;

public class GetAllRolesQueryHandler(IRoleReadRepository roleReadRepository)
    : IRequestHandler<GetAllRolesQuery, List<RoleSelectResponse>>
{
    public Task<List<RoleSelectResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        return roleReadRepository.GetAllRoleSelectsAsync(cancellationToken);
    }
}