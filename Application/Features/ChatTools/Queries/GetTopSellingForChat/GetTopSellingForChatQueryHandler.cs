using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetTopSellingForChat;

public class GetTopSellingForChatQueryHandler(IStatisticalReadRepository statisticalReadRepository)
    : IRequestHandler<GetTopSellingForChatQuery, Result<ChatToolResult<ChatTopSellingProductDto>>>
{
    public async Task<Result<ChatToolResult<ChatTopSellingProductDto>>> Handle(
        GetTopSellingForChatQuery request,
        CancellationToken cancellationToken)
    {
        var limit = ChatToolLimit.Clamp(request.Limit);
        var (start, end) = ChatToolDateRange.Resolve(request.FromDate, request.ToDate);
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
        return new ChatToolResult<ChatTopSellingProductDto>(dtos, dtos.Count, false);
    }
}
