using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Commands.DeleteMultipleRoles;

public record DeleteMultipleRolesCommand(List<string> RoleNames) : IRequest<RoleDeleteResponse>;
