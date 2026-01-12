using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Queries.GetUserPermissionsById;

public record GetUserPermissionsByIdQuery : IRequest<Result<PermissionAndRoleOfUserResponse>>
{
    public Guid? UserId { get; init; }
}
