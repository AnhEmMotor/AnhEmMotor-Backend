using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Marketing.Queries.GetProductViewHistory;

public class GetProductViewHistoryQueryHandler(IProductViewRepository repository) : IRequestHandler<GetProductViewHistoryQuery, Result<PagedResult<ProductViewHistoryResponse>>>
{
    public async Task<Result<PagedResult<ProductViewHistoryResponse>>> Handle(GetProductViewHistoryQuery request, CancellationToken cancellationToken)
    {
        var (entities, totalCount) = await repository.GetProductViewHistoryPagedAsync(
            request.SearchKeyword,
            request.PageNumber,
            request.PageSize,
            request.From,
            request.To,
            cancellationToken);

        var items = entities.Select(pv => ProductViewHistoryResponse.FromEntity(pv)).ToList();

        var pagedResult = new PagedResult<ProductViewHistoryResponse>(items, totalCount, request.PageNumber, request.PageSize);
        return Result<PagedResult<ProductViewHistoryResponse>>.Success(pagedResult);
    }
}
