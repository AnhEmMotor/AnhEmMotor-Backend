using Application.ApiContracts.Permission.Responses;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllRoles;

public record GetAllRolesQuery() : IRequest<List<RoleSelectResponse>>;
