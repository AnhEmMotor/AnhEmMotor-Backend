using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed record GetVariantLiteByProductIdQuery(int ProductId, bool IncludeDeleted) : IRequest<Result<List<ProductVariantLiteResponse>?>>;
