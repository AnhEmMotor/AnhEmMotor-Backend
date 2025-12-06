using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using MediatR;

namespace Application.Features.UserManager.Commands.AssignRoles;

public record AssignRolesCommand(Guid UserId, AssignRolesRequest Model) : IRequest<AssignRoleResponse>;
