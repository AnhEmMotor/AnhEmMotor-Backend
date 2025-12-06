using Application.ApiContracts.Permission.Responses;
using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Helpers;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Permissions.Commands.UpdateRole;

public class UpdateRoleCommandHandler(RoleManager<ApplicationRole> roleManager) : IRequestHandler<UpdateRoleCommand, RoleUpdateResponse>
{
    public async Task<RoleUpdateResponse> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleName = request.RoleName;
        var model = request.Model;

        var role = await roleManager.FindByNameAsync(roleName) ?? throw new NotFoundException("Role not found.");

        // Chỉ cập nhật Description nếu được cung cấp
        if (!string.IsNullOrWhiteSpace(model.Description))
        {
            role.Description = model.Description;
        }

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }

        return new RoleUpdateResponse()
        {
            Message = $"Role '{roleName}' updated successfully.",
            RoleId = role.Id,
            RoleName = role.Name,
            Description = role.Description
        };
    }
}
