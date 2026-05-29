using Application.ApiContracts.Loyalty.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Lead.Lead;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System;

namespace Application.Features.Loyalty.Queries.GetLoyaltyMembers
{
    public sealed class GetLoyaltyMembersQueryHandler(ILeadReadRepository leadRepository) : IRequestHandler<GetLoyaltyMembersQuery, Result<PagedResult<LoyaltyMemberResponse>>>
    {
        public async Task<Result<PagedResult<LoyaltyMemberResponse>>> Handle(
            GetLoyaltyMembersQuery request,
            CancellationToken cancellationToken)
        {
            var sieveModel = request.SieveModel ?? new SieveModel();
            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-{nameof(Lead.Points)}";
            }
            var result = await leadRepository.GetPagedAsync<LoyaltyMemberResponse>(
                sieveModel,
                DataFetchMode.ActiveOnly,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
    }
}
