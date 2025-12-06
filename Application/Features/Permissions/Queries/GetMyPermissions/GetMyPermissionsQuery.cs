using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Queries.GetMyPermissions;

public record GetMyPermissionsQuery(string? UserId) : IRequest<PermissionAndRoleOfUserResponse>;
