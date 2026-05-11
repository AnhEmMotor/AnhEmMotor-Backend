using Application.ApiContracts.Loyalty.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using MediatR;
using System;

namespace Application.Features.Loyalty.Queries.GetLoyaltyMembers
{
    public sealed class GetLoyaltyMembersQueryHandler(ILeadReadRepository leadRepository) : IRequestHandler<GetLoyaltyMembersQuery, Result<List<LoyaltyMemberResponse>>>
    {
        public async Task<Result<List<LoyaltyMemberResponse>>> Handle(
            GetLoyaltyMembersQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await leadRepository.GetLoyaltyMembersAsync(request.Search, cancellationToken)
                .ConfigureAwait(false);
            var members = entities
                .Select(
                    l => new LoyaltyMemberResponse
                    {
                        Id = l.Id,
                        FullName = l.FullName,
                        PhoneNumber = l.PhoneNumber,
                        Tier = l.Tier,
                        Points = l.Points
                    })
                .ToList();
            return members;
        }
    }
}
