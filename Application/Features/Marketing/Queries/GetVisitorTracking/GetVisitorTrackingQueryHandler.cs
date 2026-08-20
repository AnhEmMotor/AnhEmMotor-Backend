using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.Marketing.Queries.GetVisitorTracking;

public class GetVisitorTrackingQueryHandler(IProductViewRepository productViewRepository)
    : IRequestHandler<GetVisitorTrackingQuery, Result<List<DetailedProductView>>>
{
    public async Task<Result<List<DetailedProductView>>> Handle(GetVisitorTrackingQuery request, CancellationToken cancellationToken)
    {
        var views = await productViewRepository.GetDetailedRecentViewsAsync(request.Take, cancellationToken);
        return Result<List<DetailedProductView>>.Success(views);
    }
}
