using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Select;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetActiveVariantLiteList;

public sealed record GetActiveVariantLiteListQuery(ProductListRequest Request) : IRequest<PagedResult<ProductVariantLiteResponse>>;
