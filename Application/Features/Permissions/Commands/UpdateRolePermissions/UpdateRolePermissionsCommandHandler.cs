using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Authorization;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace Application.Features.Permissions.Commands.UpdateRolePermissions;

public class UpdateRolePermissionsCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRolePermissionsCommand, PermissionRoleUpdateResponse>
{
    public async Task<PermissionRoleUpdateResponse> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.RoleName;
        var model = request.Model;

        var role = await roleManager.FindByNameAsync(roleName) ?? throw new NotFoundException("Role not found.");
        bool isRoleInfoUpdated = false;
        if (model.Description is not null && role.Description != model.Description)
        {
            role.Description = model.Description;
            isRoleInfoUpdated = true;
        }

        if (model.Permissions.Count != 0)
        {
            var allPermissions = typeof(PermissionsList)
            .GetNestedTypes()
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
            .Where(permission => permission is not null)
            .ToList();

            var invalidPermissions = model.Permissions.Except(allPermissions!).ToList();
            if (invalidPermissions.Count != 0)
            {
                throw new BadRequestException($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
            }

            var currentRolePermissions = await rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken);

            var currentPermissionNames = currentRolePermissions
                .Where(rp => rp.Permission is not null)
                .Select(rp => rp.Permission!.Name)
                .ToHashSet();

            var permissionsToRemove = currentRolePermissions
                .Where(rp => rp.Permission is not null && !model.Permissions.Contains(rp.Permission.Name))
                .ToList();

            var permissionNamesToAdd = model.Permissions
                .Where(pName => !currentPermissionNames.Contains(pName))
                .ToList();

            if (permissionsToRemove.Count != 0)
            {
                var permissionIdsToRemove = permissionsToRemove
                    .Select(rp => rp.PermissionId)
                    .ToHashSet();

                var permissionsToCheck = await rolePermissionRepository.GetByPermissionIdsAsync(permissionIdsToRemove, cancellationToken);
                var orphanedPermissionNames = permissionsToCheck
                    .Where(rp => rp.Permission != null)
                    .GroupBy(rp => rp.Permission!.Name)
                    .Where(g => g.Count() == 1)
                    .Select(g => g.Key)
                    .ToList();

                if (orphanedPermissionNames.Count != 0)
                {
                    throw new BadRequestException($"Cannot remove the last role assignment for permissions: {string.Join(", ", orphanedPermissionNames)}. Assign them to another role first.");
                }
            }

            var dbPermissionsToAdd = await permissionRepository.GetPermissionsByNamesAsync(permissionNamesToAdd, cancellationToken);

            var rolePermissionsToAdd = dbPermissionsToAdd
                .Select(p => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id
                })
                .ToList();

            if (permissionsToRemove.Count != 0)
            {
                rolePermissionRepository.RemoveRange(permissionsToRemove);
                isRoleInfoUpdated = true;
            }

            if (rolePermissionsToAdd.Count != 0)
            {
                await rolePermissionRepository.AddRangeAsync(rolePermissionsToAdd, cancellationToken);
                isRoleInfoUpdated = true;
            }
        }
        

        if (isRoleInfoUpdated)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new PermissionRoleUpdateResponse();
    }
}
