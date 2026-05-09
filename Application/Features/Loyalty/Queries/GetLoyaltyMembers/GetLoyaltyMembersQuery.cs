using Application.ApiContracts.Loyalty.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead;
using MediatR;

namespace Application.Features.Loyalty.Queries.GetLoyaltyMembers;

public sealed class GetLoyaltyMembersQuery : IRequest<Result<List<LoyaltyMemberResponse>>>
{
    public string? Search { get; set; }
}



