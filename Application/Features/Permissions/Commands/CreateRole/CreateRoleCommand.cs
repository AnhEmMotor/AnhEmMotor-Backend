using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Commands.CreateRole;

public record CreateRoleCommand : IRequest<Result<RoleCreateResponse>>
{
    public string? RoleName { get; init; }

    public string? Description { get; init; }

    public List<string>? Permissions { get; init; }
}
