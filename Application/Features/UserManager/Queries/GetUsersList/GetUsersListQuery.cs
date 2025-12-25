using Application.ApiContracts.User.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed record GetUsersListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<UserDTOForManagerResponse>>;
