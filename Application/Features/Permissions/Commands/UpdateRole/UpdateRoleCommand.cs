using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.UpdateRole;

public record UpdateRoleCommand : IRequest<Result<PermissionRoleUpdateResponse>>
{
    public string? RoleName { get; init; }

    public List<string>? Permissions { get; init; } = [];

    public string? Description { get; init; }
}
