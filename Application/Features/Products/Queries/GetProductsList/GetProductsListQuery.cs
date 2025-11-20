using Application.ApiContracts.Product.Select;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed record GetProductsListQuery : IRequest<PagedResult<ProductDetailResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 5;
    public string? Search { get; init; }
    public List<string> StatusIds { get; init; } = [];

    public static GetProductsListQuery FromRequest(ProductListRequest request)
    {
        return new GetProductsListQuery
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 5,
            Search = request.Search,
            StatusIds = request.StatusIds
        };
    }
}
