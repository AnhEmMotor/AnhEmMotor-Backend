using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.DeleteRole;

public record DeleteRoleCommand(string RoleName) : IRequest<Result<RoleDeleteResponse>>;
