using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(string RoleName) : IRequest<List<PermissionResponse>>;
