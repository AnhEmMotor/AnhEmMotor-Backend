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
        var result = await repo.GetRevenueByCategoryAsync(request.Start, request.End, cancellationToken)
            .ConfigureAwait(false);
        return Result<IEnumerable<RevenueByCategoryResponse>>.Success(result);
    }
}
