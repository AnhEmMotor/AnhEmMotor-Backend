using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Queries.GetRolePermissions;

public record GetRolePermissionsQuery : IRequest<Result<List<PermissionResponse>>>
{
    public string? RoleName { get; init; }
}
