using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLowStockProductsForChat;

public class GetLowStockProductsForChatQueryHandler(
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetLowStockProductsForChatQuery, Result<ChatToolEnvelope<ChatLowStockProductDto>>>
{
    private const string InStockStatus = "Còn hàng";

    public async Task<Result<ChatToolEnvelope<ChatLowStockProductDto>>> Handle(
        GetLowStockProductsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var now = dateProvider.UtcNow;
        var performance = await statisticalReadRepository
            .GetProductPerformanceTableAsync(now.AddDays(-30), now, cancellationToken)
            .ConfigureAwait(false);
        var lowStock = performance
            .Where(p => !string.Equals(p.Status, InStockStatus, StringComparison.Ordinal))
            .OrderBy(p => p.StockQuantity)
            .ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = lowStock
            .Take(limit)
            .Select(
                p => new ChatLowStockProductDto
                {
                    ProductName = p.ProductName ?? string.Empty,
                    StockQuantity = p.StockQuantity,
                    SellPrice = p.SellPrice,
                    Status = p.Status ?? string.Empty
                })
            .ToList();
        var inner = new ChatToolResult<ChatLowStockProductDto>(dtos, lowStock.Count, lowStock.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetProductPerformanceTableAsync",
            new Dictionary<string, string> { ["Loại trừ"] = "Sản phẩm còn hàng (\"Còn hàng\")" },
            "ton-kho",
            null);
        return ChatToolEnvelope<ChatLowStockProductDto>.Wrap(inner, meta);
    }
}
