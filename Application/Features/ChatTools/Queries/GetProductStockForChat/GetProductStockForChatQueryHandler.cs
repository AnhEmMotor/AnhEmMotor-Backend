using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetProductStockForChat;

public class GetProductStockForChatQueryHandler(
    IProductReadRepository productReadRepository,
    IStatisticalReadRepository statisticalReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetProductStockForChatQuery, Result<ChatToolEnvelope<ChatProductStockDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatProductStockDto>>> Handle(
        GetProductStockForChatQuery request,
        CancellationToken cancellationToken)
    {
        var products = await productReadRepository
            .GetByIdWithVariantsAsync([request.ProductId], cancellationToken)
            .ConfigureAwait(false);
        var product = products.FirstOrDefault();
        if (product == null)
        {
            return Result<ChatToolEnvelope<ChatProductStockDto>>.Failure(Error.NotFound("Không tìm thấy sản phẩm"));
        }
        var limit = ChatToolLimit.Clamp(request.Limit);
        var variants = product.ProductVariants.Take(limit).ToList();
        var dtos = new List<ChatProductStockDto>(variants.Count);
        foreach (var variant in variants)
        {
            var stockAndPrice = await statisticalReadRepository
                .GetProductStockAndPriceAsync(variant.Id, cancellationToken)
                .ConfigureAwait(false);
            dtos.Add(
                new ChatProductStockDto
                {
                    VariantId = variant.Id,
                    VariantName = variant.VariantName,
                    UnitPrice = stockAndPrice?.UnitPrice ?? variant.Price ?? 0,
                    StockQuantity = stockAndPrice?.StockQuantity ?? 0
                });
        }
        var inner = new ChatToolResult<ChatProductStockDto>(
            dtos, product.ProductVariants.Count, product.ProductVariants.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IStatisticalReadRepository.GetProductStockAndPriceAsync",
            new Dictionary<string, string> { ["Loại trừ"] = "Hàng đang giữ cho đơn chưa giao" },
            "ton-kho",
            "VND");
        return ChatToolEnvelope<ChatProductStockDto>.Wrap(inner, meta);
    }
}
