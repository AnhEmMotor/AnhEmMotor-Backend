using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Primitives;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsByUserId;

public class GetOutputsByUserIdQueryHandler(
        IOutputReadRepository outputReadRepository,
        ISievePaginator sievePaginator)
    : IRequestHandler<GetOutputsByUserIdQuery, Result<PagedResult<MyOrderResponse>>>
{
    public async Task<Result<PagedResult<MyOrderResponse>>> Handle(
        GetOutputsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = outputReadRepository.GetQueryable();
        query = query.Where(o => o.BuyerId == request.BuyerId);

        var pagedResult = await sievePaginator.ApplyAsync<OutputEntity, MyOrderResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return pagedResult;
    }
}
