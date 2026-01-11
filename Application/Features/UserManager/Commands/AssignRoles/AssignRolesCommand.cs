using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.AssignRoles;

public record AssignRolesCommand(Guid UserId, AssignRolesRequest Model) : IRequest<Result<AssignRoleResponse>>;
