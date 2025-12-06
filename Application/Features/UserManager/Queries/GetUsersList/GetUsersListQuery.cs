using Application.ApiContracts.User.Responses;
using Domain.Shared;
using MediatR;
using Sieve.Models;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed record GetUsersListQuery(SieveModel SieveModel) : IRequest<PagedResult<UserResponse>>;
