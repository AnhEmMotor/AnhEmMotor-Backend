using Application.ApiContracts.Permission.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Commands.UpdateRole;

public class UpdateRoleCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    IPermissionReadRepository permissionRepository,
    IRoleUpdateRepository roleUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRoleCommand, RoleUpdateResponse>
{
    public async Task<RoleUpdateResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    { throw new NotImplementedException(); }
}
