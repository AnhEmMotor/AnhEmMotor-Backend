using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Primitives;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsByUserId;

public sealed class GetOutputsByUserrIdQueryHandler(IOutputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetOutputsByUserIdQuery, Result<PagedResult<OutputResponse>>>
{
    public async Task<Result<PagedResult<OutputResponse>>> Handle(
        GetOutputsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable().Where(o => o.BuyerId == request.BuyerId);

        return await paginator.ApplyAsync<OutputEntity, OutputResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
