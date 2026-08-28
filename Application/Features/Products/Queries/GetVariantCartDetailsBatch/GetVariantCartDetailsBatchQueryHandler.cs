using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.InventoryOnHand;
using Application.Interfaces.Repositories.ProductVariant;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantCartDetailsBatch;

public class GetVariantCartDetailsBatchQueryHandler(
    IProductVariantReadRepository repository,
    IInventoryOnHandReadRepository inventoryOnHandRepository) : IRequestHandler<GetVariantCartDetailsBatchQuery, Result<List<VariantCartDetailResponse>>>
{
    public async Task<Result<List<VariantCartDetailResponse>>> Handle(
        GetVariantCartDetailsBatchQuery request,
        CancellationToken cancellationToken)
    {
        var variants = (await repository.GetByIdAsync(request.VariantIds, cancellationToken).ConfigureAwait(false)).ToList();
        var onHands = await inventoryOnHandRepository.GetByVariantIdsAsync(request.VariantIds, null, null, cancellationToken).ConfigureAwait(false);
        var onHandsByVariant = onHands.GroupBy(x => x.ProductVariantId).ToDictionary(g => g.Key, g => g.ToList());

        var responses = variants
            .Select(v =>
            {
                var response = ProductMappingConfig.BuildVariantCartDetailResponse(v);
                if (onHandsByVariant.TryGetValue(v.Id, out var vOnHands))
                {
                    response.Stock = vOnHands.Sum(x => x.StockQty);
                    foreach (var colorResp in response.Colors)
                    {
                        var colorOnHand = vOnHands.FirstOrDefault(x => x.ProductVariantColorId == colorResp.Id);
                        if (colorOnHand != null)
                        {
                            colorResp.Stock = colorOnHand.StockQty;
                        }
                    }
                }
                return response;
            })
            .ToList();
        return Result<List<VariantCartDetailResponse>>.Success(responses);
    }
}
