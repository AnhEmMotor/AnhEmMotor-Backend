using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetRevenueByCategory;

public record GetRevenueByCategoryQuery(DateTimeOffset Start, DateTimeOffset End) : IRequest<Result<IEnumerable<RevenueByCategoryResponse>>>;

public class GetRevenueByCategoryQueryHandler(IStatisticalReadRepository repo) : IRequestHandler<GetRevenueByCategoryQuery, Result<IEnumerable<RevenueByCategoryResponse>>>
{
    public async Task<Result<IEnumerable<RevenueByCategoryResponse>>> Handle(
        GetRevenueByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repo.GetRevenueByCategoryAsync(
                NormalizeToUtc(request.Start),
                NormalizeToUtc(request.End),
                cancellationToken)
            .ConfigureAwait(false);
        return Result<IEnumerable<RevenueByCategoryResponse>>.Success(result);
    }

    private static DateTimeOffset NormalizeToUtc(DateTimeOffset value) =>
        value.TimeOfDay == TimeSpan.Zero
            ? new DateTimeOffset(value.Year, value.Month, value.Day, 0, 0, 0, TimeSpan.Zero)
            : value.ToUniversalTime();
}
