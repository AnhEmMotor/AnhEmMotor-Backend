using Application.ApiContracts.Product.Responses;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using Mapster;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForOutput
{
    public class GetActiveVariantLiteListForOutputQueryHandler(IProductVariantReadRepository repository) : IRequestHandler<GetActiveVariantLiteListForOutputQuery, PagedResult<ProductVariantLiteResponseForInput>>
    {
        public async Task<PagedResult<ProductVariantLiteResponseForInput>> Handle(
            GetActiveVariantLiteListForOutputQuery request,
            CancellationToken cancellationToken)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Max(request.PageSize, 1);

            var (variants, totalCount) = await repository.GetPagedVariantsAsync(page, pageSize, cancellationToken)
                .ConfigureAwait(false);

            var responses = variants.Select(v => v.Adapt<ProductVariantLiteResponseForInput>()).ToList();

            return new PagedResult<ProductVariantLiteResponseForInput>(responses, totalCount, page, pageSize);
        }
    }
}
