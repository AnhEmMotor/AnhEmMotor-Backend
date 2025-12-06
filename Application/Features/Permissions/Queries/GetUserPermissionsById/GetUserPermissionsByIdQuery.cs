using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Queries.GetUserPermissionsById;

public record GetUserPermissionsByIdQuery(Guid UserId) : IRequest<PermissionAndRoleOfUserResponse>;
