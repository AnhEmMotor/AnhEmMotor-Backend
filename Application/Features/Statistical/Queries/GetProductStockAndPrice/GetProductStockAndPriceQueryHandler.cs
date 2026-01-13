using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductStockAndPrice;

public sealed class GetProductStockAndPriceQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetProductStockAndPriceQuery, Result<ProductStockPriceResponse?>>
{
    public async Task<Result<ProductStockPriceResponse?>> Handle(
        GetProductStockAndPriceQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetProductStockAndPriceAsync(request.VariantId, cancellationToken).ConfigureAwait(false);
        return result;
    }
}
