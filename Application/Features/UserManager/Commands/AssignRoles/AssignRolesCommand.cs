using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.AssignRoles;

public record AssignRolesCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    public Guid UserId { get; init; }

    public List<Guid>? RoleIds { get; init; }
}
