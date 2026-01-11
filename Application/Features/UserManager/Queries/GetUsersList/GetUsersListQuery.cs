using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.UserManager.Queries.GetUsersList;

public sealed record GetUsersListQuery(SieveModel SieveModel) : IRequest<Result<PagedResult<UserDTOForManagerResponse>>>;
