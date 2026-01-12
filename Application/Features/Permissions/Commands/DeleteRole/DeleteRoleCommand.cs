using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.DeleteRole;

public record DeleteRoleCommand : IRequest<Result<RoleDeleteResponse>>
{
    public string? RoleName { get; init; }
}
