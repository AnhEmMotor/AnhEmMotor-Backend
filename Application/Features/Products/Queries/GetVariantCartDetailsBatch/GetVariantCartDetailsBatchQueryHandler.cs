using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.ProductVariant;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantCartDetailsBatch;

public class GetVariantCartDetailsBatchQueryHandler(IProductVariantReadRepository repository) : IRequestHandler<GetVariantCartDetailsBatchQuery, Result<List<VariantCartDetailResponse>>>
{
    public async Task<Result<List<VariantCartDetailResponse>>> Handle(
        GetVariantCartDetailsBatchQuery request,
        CancellationToken cancellationToken)
    {
        var variants = await repository.GetByIdAsync(request.VariantIds, cancellationToken).ConfigureAwait(false);

        var responses = variants
            .Select(ProductMappingConfig.BuildVariantCartDetailResponse)
            .ToList();

        return Result<List<VariantCartDetailResponse>>.Success(responses);
    }
}
