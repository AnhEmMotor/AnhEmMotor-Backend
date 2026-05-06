using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed record GetProductCategoriesListQuery : IRequest<Result<PagedResult<ProductCategoryResponse>>>
{
    public SieveModel? SieveModel { get; init; }

    /// <summary>
    /// Nếu true, chỉ trả về thể loại sản phẩm (CategoryGroup="Product"), loại bỏ loại xe.
    /// </summary>
    public bool ProductOnly { get; init; } = false;
}

