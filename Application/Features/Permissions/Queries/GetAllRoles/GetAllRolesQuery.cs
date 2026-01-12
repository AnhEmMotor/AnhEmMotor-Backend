using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Permissions.Queries.GetAllRoles;

public record GetAllRolesQuery : IRequest<Result<List<RoleSelectResponse>>>;
