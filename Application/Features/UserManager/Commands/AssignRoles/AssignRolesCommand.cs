using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.AssignRoles;

public record AssignRolesCommand : IRequest<Result<AssignRoleResponse>>
{
    public Guid UserId { get; init; }
    public List<string>? RoleNames { get; init; }
}
