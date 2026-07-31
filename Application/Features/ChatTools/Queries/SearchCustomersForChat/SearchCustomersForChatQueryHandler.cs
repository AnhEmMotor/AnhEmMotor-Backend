using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Lead.Lead;
using MediatR;

namespace Application.Features.ChatTools.Queries.SearchCustomersForChat;

public class SearchCustomersForChatQueryHandler(
    ILeadReadRepository leadReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<SearchCustomersForChatQuery, Result<ChatToolEnvelope<ChatCustomerSearchResultDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatCustomerSearchResultDto>>> Handle(
        SearchCustomersForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var leads = await leadReadRepository.GetLoyaltyMembersAsync(request.Keyword, cancellationToken)
            .ConfigureAwait(false);

        var totalCount = leads.Count;
        var dtos = leads
            .Take(limit)
            .Select(
                l => new ChatCustomerSearchResultDto
                {
                    CustomerId = l.Id,
                    Name = l.FullName ?? string.Empty,
                    PhoneNumber = l.PhoneNumber
                })
            .ToList();

        var inner = new ChatToolResult<ChatCustomerSearchResultDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ILeadReadRepository.GetLoyaltyMembersAsync",
            new Dictionary<string, string>(),
            "tim-khach-hang",
            null);
        return ChatToolEnvelope<ChatCustomerSearchResultDto>.Wrap(inner, meta);
    }
}
