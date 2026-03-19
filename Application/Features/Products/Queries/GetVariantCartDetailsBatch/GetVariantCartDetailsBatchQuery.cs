using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantCartDetailsBatch;

public class GetVariantCartDetailsBatchQuery : IRequest<Result<List<VariantCartDetailResponse>>>
{
    public List<int> VariantIds { get; init; } = [];
}
