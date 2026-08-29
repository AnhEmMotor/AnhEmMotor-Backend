using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Features.Products.Mappings;
using Application.Interfaces.Repositories.InventoryOnHand;
using Application.Interfaces.Repositories.PredefinedOption;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForOutput
{
    public class GetActiveVariantLiteListForOutputQueryHandler(
        IProductVariantReadRepository repository,
        IInventoryOnHandReadRepository inventoryOnHandRepository,
        IPredefinedOptionReadRepository predefinedOptionReadRepository) : IRequestHandler<GetActiveVariantLiteListForOutputQuery, Result<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>>
    {
        public async Task<Result<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>> Handle(
            GetActiveVariantLiteListForOutputQuery request,
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

            var variantIds = variants.Select(v => v.Id).ToList();
            var onHands = await inventoryOnHandRepository.GetByVariantIdsAsync(variantIds, null, null, cancellationToken).ConfigureAwait(false);
            var onHandsByVariant = onHands.GroupBy(x => x.ProductVariantId).ToDictionary(g => g.Key, g => g.ToList());

            var responses = variants
                .Select(v =>
                {
                    var response = ProductMappingConfig.BuildVariantLiteResponseForInventoryReceipt(v, translations);
                    if (onHandsByVariant.TryGetValue(v.Id, out var vOnHands))
                    {
                        response.Stock = Math.Max(0, vOnHands.Sum(x => x.StockQty - x.OrderedQty));
                        foreach (var colorResp in response.Colors)
                        {
                            var colorOnHand = vOnHands.FirstOrDefault(x => x.ProductVariantColorId == colorResp.Id);
                            if (colorOnHand != null)
                            {
                                colorResp.Stock = Math.Max(0, colorOnHand.StockQty - colorOnHand.OrderedQty);
                            }
                        }
                    }
                    return response;
                })
                .ToList();
            return new PagedResult<ProductVariantLiteResponseForInventoryReceipt>(responses, totalCount, page, pageSize);
        }
    }
}
