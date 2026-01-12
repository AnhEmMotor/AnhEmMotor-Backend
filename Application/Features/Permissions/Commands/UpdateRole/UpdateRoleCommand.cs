using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.UpdateRolePermissions;

public record UpdateRoleCommand: IRequest<Result<PermissionRoleUpdateResponse>>
{
    public string? RoleName { get; init; }
    public List<string>? Permissions { get; set; } = [];
    public string? Description { get; set; }
}
