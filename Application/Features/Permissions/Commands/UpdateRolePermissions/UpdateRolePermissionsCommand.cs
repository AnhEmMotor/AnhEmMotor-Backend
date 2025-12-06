using Application.ApiContracts.Permission.Requests;
using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Commands.UpdateRolePermissions;

public record UpdateRolePermissionsCommand(string RoleName, UpdateRoleRequest Model) : IRequest<PermissionRoleUpdateResponse>;
