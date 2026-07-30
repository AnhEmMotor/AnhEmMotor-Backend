using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetTopSellingForChat;

public class GetTopSellingForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetTopSellingForChatQuery, Result<ChatToolEnvelope<ChatTopSellingProductDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatTopSellingProductDto>>> Handle(
        GetTopSellingForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate, dateProvider);
        var topProducts = await statisticalReadRepository.GetTopProductsByRevenueAsync(start, end, limit, cancellationToken)
            .ConfigureAwait(false);
        var dtos = topProducts
            .Select(
                p => new ChatTopSellingProductDto
                {
                    ProductName = p.ProductName ?? string.Empty,
                    UnitsSold = p.UnitsSold,
                    Revenue = p.Revenue
                })
            .ToList();
        var inner = new ChatToolResult<ChatTopSellingProductDto>(dtos, dtos.Count, false);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetTopProductsByRevenueAsync",
            new Dictionary<string, string>
            {
                ["Loại trừ"] = "Đơn huỷ, đơn nháp, bản ghi soft-delete",
                ["Khoảng thời gian"] = ChatToolDateRange.FormatVietnamRange(start, end)
            },
            "doanh-thu",
            "VND");
        return ChatToolEnvelope<ChatTopSellingProductDto>.Wrap(inner, meta);
    }
}
