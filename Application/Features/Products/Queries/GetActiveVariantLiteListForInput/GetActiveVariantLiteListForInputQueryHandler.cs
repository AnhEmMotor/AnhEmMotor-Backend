using Application.Features.Products.Queries.GetActiveVariantLiteList;
using Application.Interfaces.Repositories.ProductVariant;
using Sieve.Models;
using Mapster;
using Application.ApiContracts.Product.Responses;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForInvoices
{
    public class GetActiveVariantLiteListForInputQueryHandler(IProductVariantReadRepository repository)
    {
        public async Task<Domain.Primitives.PagedResult<ProductVariantLiteResponseForInput>> Handle(
        GetActiveVariantLiteListForManagerQuery request,
        CancellationToken cancellationToken)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Max(request.PageSize, 1);

            var (variants, totalCount) = await repository.GetPagedVariantsAsync(page, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponseForInput>()).ToList();

            return new Domain.Primitives.PagedResult<ProductVariantLiteResponseForInput>(responses, totalCount, page, pageSize);
        }
    }
}
