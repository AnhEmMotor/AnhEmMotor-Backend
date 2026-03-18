using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductStoreDetailBySlug;

public sealed record GetProductStoreDetailBySlugQuery(string Slug) : IRequest<Result<ProductStoreDetailResponse>>;
