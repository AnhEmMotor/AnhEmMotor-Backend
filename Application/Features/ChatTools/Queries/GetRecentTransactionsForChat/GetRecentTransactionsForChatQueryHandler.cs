using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRecentTransactionsForChat;

public class GetRecentTransactionsForChatQueryHandler(
    IStatisticalAnalyticsRepository statisticalAnalyticsRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetRecentTransactionsForChatQuery, Result<ChatToolEnvelope<ChatRecentTransactionDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatRecentTransactionDto>>> Handle(
        GetRecentTransactionsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var transactions = await statisticalAnalyticsRepository
            .GetRecentTransactionsAsync(limit, cancellationToken)
            .ConfigureAwait(false);
        var dtos = transactions
            .Select(
                t => new ChatRecentTransactionDto
                {
                    Timestamp = t.Timestamp,
                    CustomerName = t.CustomerName,
                    ProductName = t.ProductName,
                    Amount = t.Amount,
                    IsRevenue = t.IsRevenue,
                    Status = t.Status,
                    StaffName = t.StaffName
                })
            .ToList();
        var inner = new ChatToolResult<ChatRecentTransactionDto>(dtos, dtos.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalAnalyticsRepository.GetRecentTransactionsAsync",
            new Dictionary<string, string>(),
            "giao-dich-gan-day",
            "VND");
        return ChatToolEnvelope<ChatRecentTransactionDto>.Wrap(inner, meta);
    }
}
