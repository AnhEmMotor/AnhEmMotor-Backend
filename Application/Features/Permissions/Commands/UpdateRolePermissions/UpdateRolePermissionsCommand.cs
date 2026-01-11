using Application.ApiContracts.Permission.Requests;
using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.UpdateRolePermissions;

public record UpdateRolePermissionsCommand(string RoleName, UpdateRoleRequest Model) : IRequest<Result<PermissionRoleUpdateResponse>>;
