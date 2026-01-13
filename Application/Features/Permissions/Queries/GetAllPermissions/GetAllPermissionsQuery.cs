using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllPermissions;

public record GetAllPermissionsQuery : IRequest<Result<Dictionary<string, List<PermissionResponse>>>>;
