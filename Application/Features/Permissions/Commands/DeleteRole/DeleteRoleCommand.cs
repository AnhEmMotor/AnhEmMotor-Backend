using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Commands.DeleteRole;

public record DeleteRoleCommand(string RoleName) : IRequest<RoleDeleteResponse>;
