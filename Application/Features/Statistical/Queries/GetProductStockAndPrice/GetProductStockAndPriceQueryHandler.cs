using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductStockAndPrice;

public sealed class GetProductStockAndPriceQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetProductStockAndPriceQuery, ProductStockPriceResponse?>
{
    public Task<ProductStockPriceResponse?> Handle(GetProductStockAndPriceQuery request, CancellationToken cancellationToken)
    { return repository.GetProductStockAndPriceAsync(request.VariantId, cancellationToken); }
}
