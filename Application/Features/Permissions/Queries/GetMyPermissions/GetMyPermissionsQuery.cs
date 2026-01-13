using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Queries.GetMyPermissions;

public record GetMyPermissionsQuery : IRequest<Result<PermissionAndRoleOfUserResponse>>
{
    public string? UserId { get; init; }
}
