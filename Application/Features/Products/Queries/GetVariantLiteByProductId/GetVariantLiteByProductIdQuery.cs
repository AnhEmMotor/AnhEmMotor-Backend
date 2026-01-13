using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed record GetVariantLiteByProductIdQuery : IRequest<Result<List<ProductVariantLiteResponse>?>>
{
    public int ProductId { get; init; }
    public bool IncludeDeleted { get; init; }
}