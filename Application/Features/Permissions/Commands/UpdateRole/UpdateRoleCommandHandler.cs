using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
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
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRoleCommand, Result<RoleUpdateResponse>>
{
    public async Task<Result<RoleUpdateResponse>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    { throw new NotImplementedException(); }
}
