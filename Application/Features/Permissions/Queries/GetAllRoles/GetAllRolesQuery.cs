using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Permissions.Queries.GetAllRoles;

public class GetAllRolesQuery : SieveModel, IRequest<Result<PagedResult<RoleSelectResponse>>>;
