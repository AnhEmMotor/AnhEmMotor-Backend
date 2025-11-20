using Application.ApiContracts.Product.Select;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed record GetDeletedProductsListQuery(ProductListRequest Request) : IRequest<PagedResult<ProductDetailResponse>>;
