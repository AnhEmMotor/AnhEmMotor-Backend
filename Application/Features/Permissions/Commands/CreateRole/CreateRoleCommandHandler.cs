using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Authorization;
using Domain.Constants.Security;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace Application.Features.Permissions.Commands.CreateRole;

public class CreateRoleCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateRoleCommand, RoleCreateResponse>
{
    public async Task<RoleCreateResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var model = request.Model;

        var roleExists = await roleManager.RoleExistsAsync(model.RoleName).ConfigureAwait(false);
        if(roleExists)
        {
            throw new BadRequestException("Role already exists.");
        }

        if(model.Permissions is null || model.Permissions.Count == 0)
        {
            throw new BadRequestException("At least one permission must be assigned to the role.");
        }

        var allPermissions = typeof(PermissionsList)
            .GetNestedTypes()
            .SelectMany(
                type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
            .Where(permission => permission is not null)
            .ToList();

        var invalidPermissions = model.Permissions.Except(allPermissions!).ToList();
        if(invalidPermissions.Count != 0)
        {
            throw new BadRequestException($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        }

        var role = new ApplicationRole { Name = model.RoleName, Description = model.Description };

        var createResult = await roleManager.CreateAsync(role).ConfigureAwait(false);
        if(!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }

        var permissionsInDb = await permissionRepository.GetPermissionsByNamesAsync(
            model.Permissions,
            cancellationToken)
            .ConfigureAwait(false);

        var rolePermissions = permissionsInDb
            .Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id })
            .ToList();

        await rolePermissionRepository.AddRangeAsync(rolePermissions, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RoleCreateResponse()
        {
            Message = $"Role '{model.RoleName}' created successfully with {rolePermissions.Count} permission(s).",
            RoleId = role.Id,
            RoleName = role.Name,
            Description = role.Description,
            Permissions = model.Permissions
        };
    }
}
