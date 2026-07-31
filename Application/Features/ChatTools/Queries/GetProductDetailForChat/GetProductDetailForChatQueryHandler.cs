using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetProductDetailForChat;

public class GetProductDetailForChatQueryHandler(
    IProductReadRepository productReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetProductDetailForChatQuery, Result<ChatToolEnvelope<ChatProductDetailDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatProductDetailDto>>> Handle(
        GetProductDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productReadRepository.GetByIdWithDetailsAsync(request.ProductId, cancellationToken)
            .ConfigureAwait(false);
        if (product == null)
        {
            return Result<ChatToolEnvelope<ChatProductDetailDto>>.Failure(Error.NotFound("Không tìm thấy sản phẩm"));
        }

        var variants = product.ProductVariants
            .Select(
                v => new ChatProductVariantDetailDto
                {
                    VariantId = v.Id,
                    VariantName = v.VariantName,
                    Sku = v.SKU,
                    Price = v.Price
                })
            .ToList();

        var dto = new ChatProductDetailDto
        {
            ProductId = product.Id,
            ProductName = product.Name ?? string.Empty,
            ShortDescription = product.ShortDescription,
            BrandName = product.Brand?.Name,
            CategoryName = product.ProductCategory?.Name,
            PriceFrom = product.ProductVariants.Count > 0 ? product.ProductVariants.Min(v => v.Price) : null,
            PriceTo = product.ProductVariants.Count > 0 ? product.ProductVariants.Max(v => v.Price) : null,
            Variants = variants
        };

        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IProductReadRepository.GetByIdWithDetailsAsync",
            new Dictionary<string, string>(),
            "san-pham",
            null);
        return ChatToolEnvelope<ChatProductDetailDto>.WrapSingle(dto, meta);
    }
}
