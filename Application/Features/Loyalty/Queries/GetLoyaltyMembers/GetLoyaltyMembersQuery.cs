using Application.ApiContracts.Loyalty.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Loyalty.Queries.GetLoyaltyMembers;

public sealed class GetLoyaltyMembersQuery : IRequest<Result<PagedResult<LoyaltyMemberResponse>>>
{
    public SieveModel? SieveModel { get; set; }
}

