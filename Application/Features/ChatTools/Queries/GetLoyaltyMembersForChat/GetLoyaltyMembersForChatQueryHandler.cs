using Application.ApiContracts.Loyalty.Responses;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Lead.Lead;
using Domain.Constants;
using MediatR;
using Sieve.Models;

namespace Application.Features.ChatTools.Queries.GetLoyaltyMembersForChat;

public class GetLoyaltyMembersForChatQueryHandler(
    ILeadReadRepository leadReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetLoyaltyMembersForChatQuery, Result<ChatToolEnvelope<ChatLoyaltyMemberDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatLoyaltyMemberDto>>> Handle(
        GetLoyaltyMembersForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var sieveModel = new SieveModel
        {
            Sorts = $"-{nameof(LoyaltyMemberResponse.Points)}",
            Page = 1,
            PageSize = limit
        };
        var paged = await leadReadRepository
            .GetPagedAsync<LoyaltyMemberResponse>(
                sieveModel,
                DataFetchMode.ActiveOnly,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var dtos = (paged.Items ?? [])
            .Select(
                m => new ChatLoyaltyMemberDto
                {
                    LeadId = m.Id,
                    FullName = m.FullName,
                    PhoneNumber = m.PhoneNumber,
                    Tier = m.Tier,
                    Points = m.Points
                })
            .ToList();
        var totalCount = (int)(paged.TotalCount ?? dtos.Count);
        var inner = new ChatToolResult<ChatLoyaltyMemberDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ILeadReadRepository.GetPagedAsync<LoyaltyMemberResponse>",
            new Dictionary<string, string> { ["Sắp xếp"] = "Điểm tích lũy giảm dần" },
            "thanh-vien-thanh-than",
            null);
        return ChatToolEnvelope<ChatLoyaltyMemberDto>.Wrap(inner, meta);
    }
}
