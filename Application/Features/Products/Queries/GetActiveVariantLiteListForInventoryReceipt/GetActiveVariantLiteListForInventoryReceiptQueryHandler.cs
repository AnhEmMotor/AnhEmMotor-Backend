using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.PredefinedOption;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForInventoryReceipt
{
    public class GetActiveVariantLiteListForInventoryReceiptQueryHandler(
        IProductVariantReadRepository repository,
        IPredefinedOptionReadRepository predefinedOptionReadRepository) : IRequestHandler<GetActiveVariantLiteListForInventoryReceiptQuery, Result<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>>
    {
        public async Task<Result<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>> Handle(
            GetActiveVariantLiteListForInventoryReceiptQuery request,
            CancellationToken cancellationToken)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Max(request.PageSize, 1);
            var translations = await predefinedOptionReadRepository
                .GetAllAsDictionaryAsync(cancellationToken)
                .ConfigureAwait(false);
            var (variants, totalCount) = await repository.GetPagedVariantsAsync(
                page,
                pageSize,
                request.Filters,
                request.Sorts,
                cancellationToken,
                search: request.Search)
                .ConfigureAwait(false);
            var responses = variants
                .Select(v => ProductMappingConfig.BuildVariantLiteResponseForInventoryReceipt(v, translations))
                .ToList();
            return new PagedResult<ProductVariantLiteResponseForInventoryReceipt>(responses, totalCount, page, pageSize);
        }
    }
}
