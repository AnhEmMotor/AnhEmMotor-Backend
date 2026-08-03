using Application.Common.Models;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.StoreChat.Queries.GetProductVariantsForStaff;

public class GetProductVariantsForStaffQueryHandler(IProductReadRepository productReadRepository)
    : IRequestHandler<GetProductVariantsForStaffQuery, Result<List<StoreChatVariantCardDto>>>
{
    public async Task<Result<List<StoreChatVariantCardDto>>> Handle(
        GetProductVariantsForStaffQuery request, CancellationToken cancellationToken)
    {
        var product = await productReadRepository.GetByIdWithDetailsAsync(request.ProductId, cancellationToken)
            .ConfigureAwait(false);
        if (product == null)
        {
            return Error.NotFound("Không tìm thấy sản phẩm.");
        }

        return product.ProductVariants.Select(v => new StoreChatVariantCardDto
        {
            VariantId = v.Id,
            VariantName = v.VariantName,
            ProductName = product.Name,
            Sku = v.SKU,
            Price = v.Price,
            Slug = v.UrlSlug,
            Colors = v.ProductVariantColors.Select(c => new StoreChatVariantColorDto
            {
                ColorId = c.Id,
                ColorName = c.ColorName,
                ColorCode = c.ColorCode,
                ImageUrl = c.CoverImageUrl
            }).ToList()
        }).ToList();
    }
}
