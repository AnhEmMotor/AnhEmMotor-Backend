using Application.ApiContracts.Permission.Requests;
using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Commands.CreateRole;

public record CreateRoleCommand(CreateRoleRequest Model) : IRequest<RoleCreateResponse>;
