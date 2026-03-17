using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Products.Queries.GetProductsListForPriceManagement;

public sealed class GetProductsListForPriceManagementQueryHandler(IProductReadRepository repository) : IRequestHandler<GetProductsListForPriceManagementQuery, Result<PagedResult<ProductPriceLiteResponse>>>
{
    public async Task<Result<PagedResult<ProductPriceLiteResponse>>> Handle(
        GetProductsListForPriceManagementQuery request,
        CancellationToken cancellationToken)
    {
        var sieveModel = request.SieveModel ?? new SieveModel();

        var pagedResult = await repository.GetPagedProductsForPriceManagementAsync(
            sieveModel.Page ?? 1,
            sieveModel.PageSize ?? 10,
            sieveModel.Filters,
            sieveModel.Sorts,
            cancellationToken)
            .ConfigureAwait(false);

        var productsEntities = pagedResult.Items;
        var totalCount = pagedResult.TotalCount;

        var products = productsEntities.Select(
            p => new ProductPriceLiteResponse
            {
                Id = p.Id,
                Name = p.Name,
                Variants =
                    [ .. p.ProductVariants
                            .Select(
                                v => new ProductVariantPriceLiteResponse
                            {
                                Id = v.Id,
                                Name =
                                    v.VariantOptionValues.Count > 0
                                                    ? string.Join(
                                                        " / ",
                                                        v.VariantOptionValues
                                                            .Select(vov => vov.OptionValue?.Name ?? string.Empty)
                                                            .Where(n => !string.IsNullOrEmpty(n)))
                                                    : "Mặc định",
                                Price = v.Price ?? 0
                            }) ]
            })
            .ToList();

        return Result<PagedResult<ProductPriceLiteResponse>>.Success(
            new PagedResult<ProductPriceLiteResponse>(
                products,
                totalCount,
                sieveModel.Page ?? 1,
                sieveModel.PageSize ?? 10));
    }
}
