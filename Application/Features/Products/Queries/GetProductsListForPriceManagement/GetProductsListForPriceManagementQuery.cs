using Application.Common.Models;
using Application.ApiContracts.Product.Responses;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Products.Queries.GetProductsListForPriceManagement;

public sealed record GetProductsListForPriceManagementQuery : IRequest<Result<PagedResult<ProductPriceLiteResponse>>>
{
    public SieveModel? SieveModel { get; init; }

    public static GetProductsListForPriceManagementQuery FromRequest(SieveModel request) => new()
    {
        SieveModel = request
    };
}
