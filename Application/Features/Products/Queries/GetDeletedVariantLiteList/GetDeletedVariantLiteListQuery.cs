using Application.ApiContracts.Product.Common;
using Application.ApiContracts.Product.Select;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetDeletedVariantLiteList;

public sealed record GetDeletedVariantLiteListQuery(ProductListRequest Request) : IRequest<PagedResult<ProductVariantLiteResponse>>;
