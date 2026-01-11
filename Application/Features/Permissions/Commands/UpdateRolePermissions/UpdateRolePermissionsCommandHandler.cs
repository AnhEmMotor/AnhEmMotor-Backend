using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace Application.Features.Permissions.Commands.UpdateRolePermissions;

public class UpdateRolePermissionsCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    IRoleReadRepository roleReadRepository,
    IRoleUpdateRepository roleUpdateRepository,
    IPermissionReadRepository permissionReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRolePermissionsCommand, Result<PermissionRoleUpdateResponse>>
{
    public async Task<Result<PermissionRoleUpdateResponse>> Handle(
        UpdateRolePermissionsCommand request,
        CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByNameAsync(request.RoleName).ConfigureAwait(false) ?? return Error.NotFound("Role not found.");
        var isRoleUpdated = false;

        if(request.Model.Description is not null && string.Compare(role.Description, request.Model.Description) != 0)
        {
            role.Description = request.Model.Description;
            isRoleUpdated = true;
        }

        if(request.Model.Permissions.Count == 0)
        {
            if(isRoleUpdated)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return new PermissionRoleUpdateResponse();
        }

        var validSystemPermissions = typeof(Domain.Constants.Permission.PermissionsList)
            .GetNestedTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(fi => fi.GetRawConstantValue() as string)
            .Where(p => p is not null)
            .ToHashSet();

        var invalidPermissions = request.Model.Permissions.Where(p => !validSystemPermissions.Contains(p)).ToList();
        if(invalidPermissions.Count != 0)
        {
                        return Error.BadRequest($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        }

        var currentRolePermissions = await roleReadRepository.GetRolePermissionsByRoleIdAsync(
            role.Id,
            cancellationToken)
            .ConfigureAwait(false);

        var currentPermissionNames = currentRolePermissions
            .Where(rp => rp.Permission is not null)
            .Select(rp => rp.Permission!.Name)
            .ToHashSet();

        var permissionNamesToAdd = request.Model.Permissions.Where(p => !currentPermissionNames.Contains(p)).ToList();

        var rolePermissionsToRemove = currentRolePermissions
            .Where(rp => rp.Permission is not null && !request.Model.Permissions.Contains(rp.Permission.Name))
            .ToList();

        if(rolePermissionsToRemove.Count != 0)
        {
            var permissionIdsToRemove = rolePermissionsToRemove.Select(rp => rp.PermissionId).ToHashSet();

            var existingAssignments = await permissionReadRepository.GetRolePermissionsByPermissionIdsAsync(
                permissionIdsToRemove,
                cancellationToken)
                .ConfigureAwait(false);

            var orphanedPermissions = existingAssignments
                .GroupBy(rp => rp.PermissionId)
                .Where(g => g.Count() == 1)
                .Select(g => g.First().Permission!.Name)
                .ToList();

            if(orphanedPermissions.Count != 0)
            {
                        return Error.BadRequest($"Cannot remove last role assignment for: {string.Join(", ", orphanedPermissions)}.");
            }

            roleUpdateRepository.RemovePermissionsFromRole(rolePermissionsToRemove);
            isRoleUpdated = true;
        }

        if(permissionNamesToAdd.Count != 0)
        {
            var permissionsEntities = await permissionReadRepository.GetPermissionsByNamesAsync(
                permissionNamesToAdd,
                cancellationToken)
                .ConfigureAwait(false);

            var newRolePermissions = permissionsEntities
                .Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id })
                .ToList();

            await roleUpdateRepository.AddPermissionsToRoleAsync(newRolePermissions, cancellationToken)
                .ConfigureAwait(false);

            isRoleUpdated = true;
        }

        if(isRoleUpdated)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return new PermissionRoleUpdateResponse();
    }
}