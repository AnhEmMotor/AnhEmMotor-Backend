using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(string RoleName) : IRequest<Result<List<PermissionResponse>>>;
