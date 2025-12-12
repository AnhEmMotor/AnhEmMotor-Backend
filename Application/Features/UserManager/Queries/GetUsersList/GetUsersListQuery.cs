using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using MediatR;
using Sieve.Models;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed record GetUsersListQuery(SieveModel SieveModel) : IRequest<Domain.Primitives.PagedResult<UserDTOForManagerResponse>>;
