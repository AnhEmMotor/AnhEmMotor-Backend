using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<Dictionary<string, List<PermissionResponse>>>;
