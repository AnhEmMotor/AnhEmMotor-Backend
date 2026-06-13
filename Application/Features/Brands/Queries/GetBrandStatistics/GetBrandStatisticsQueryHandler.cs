using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Brand;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandStatistics;

public class GetBrandStatisticsQueryHandler(IBrandReadRepository repository) : IRequestHandler<GetBrandStatisticsQuery, Result<BrandStatisticsResponse>>
{
    public async Task<Result<BrandStatisticsResponse>> Handle(
        GetBrandStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await repository.GetStatisticsAsync(cancellationToken).ConfigureAwait(false);
        return stats;
    }
}
