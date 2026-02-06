using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.UserManager.Queries.GetUsersList;

public record GetUsersListQuery : IRequest<Result<PagedResult<UserDTOForManagerResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
