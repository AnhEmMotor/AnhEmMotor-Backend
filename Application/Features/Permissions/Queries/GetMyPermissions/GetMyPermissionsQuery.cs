using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Permissions.Queries.GetMyPermissions;

public record GetMyPermissionsQuery : IRequest<Result<PermissionAndRoleOfUserResponse>>
{
    [JsonIgnore]
    public string? UserId { get; init; }
}
