using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetProductStockForChat;

public class GetProductStockForChatQueryHandler(
    IProductReadRepository productReadRepository,
    IStatisticalReadRepository statisticalReadRepository)
    : IRequestHandler<GetProductStockForChatQuery, Result<ChatToolResult<ChatProductStockDto>>>
{
    public async Task<Result<ChatToolResult<ChatProductStockDto>>> Handle(
        GetProductStockForChatQuery request,
        CancellationToken cancellationToken)
    {
        var products = await productReadRepository
            .GetByIdWithVariantsAsync([request.ProductId], cancellationToken)
            .ConfigureAwait(false);
        var product = products.FirstOrDefault();
        if (product == null)
        {
            return Result<ChatToolResult<ChatProductStockDto>>.Failure(Error.NotFound("Không tìm thấy sản phẩm"));
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
        return new ChatToolResult<ChatProductStockDto>(dtos, product.ProductVariants.Count, product.ProductVariants.Count > dtos.Count);
    }
}
