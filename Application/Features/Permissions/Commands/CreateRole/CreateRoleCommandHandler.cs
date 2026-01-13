using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using MediatR;
using System.Reflection;

namespace Application.Features.Permissions.Commands.CreateRole;

public class CreateRoleCommandHandler(
    IRoleReadRepository roleReadRepository,
    IRoleInsertRepository roleInsertRepository,
    IPermissionReadRepository permissionRepository,
    IRoleUpdateRepository roleUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateRoleCommand, Result<RoleCreateResponse>>
{
    public async Task<Result<RoleCreateResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleExists = await roleReadRepository.IsRoleExistAsync(request.RoleName!, cancellationToken).ConfigureAwait(false);
        if(roleExists)
        {
            return Error.BadRequest("Role already exists.");
        }

        if(request.Permissions is null || request.Permissions.Count == 0)
        {
            return Error.BadRequest("At least one permission must be assigned to the role.");
        }

        var allPermissions = typeof(Domain.Constants.Permission.PermissionsList)
            .GetNestedTypes()
            .SelectMany(
                type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
            .Where(permission => permission is not null)
            .ToList();

        var invalidPermissions = request.Permissions.Except(allPermissions!).ToList();
        if(invalidPermissions.Count != 0)
        {
            return Error.BadRequest($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        }

        var role = new ApplicationRole { Name = request.RoleName, Description = request.Description };

        var createResult = await roleInsertRepository.CreateAsync(role, cancellationToken).ConfigureAwait(false);
        if(!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return Error.BadRequest(errors);
        }

        var permissionsInDb = await permissionRepository.GetPermissionsByNamesAsync(
            request.Permissions,
            cancellationToken)
            .ConfigureAwait(false);

        var rolePermissions = permissionsInDb
            .Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id })
            .ToList();

        await roleUpdateRepository.AddPermissionsToRoleAsync(rolePermissions, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RoleCreateResponse()
        {
            Message = $"Role '{request.RoleName}' created successfully with {rolePermissions.Count} permission(s).",
            RoleId = role.Id,
            RoleName = role.Name,
            Description = role.Description,
            Permissions = request.Permissions
        };
    }
}
