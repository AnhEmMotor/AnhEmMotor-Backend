using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Primitives;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsByUserId;

public sealed class GetOutputsByUserIdQueryHandler(IOutputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetOutputsByUserIdQuery, Result<PagedResult<OutputItemResponse>>>
{
    public async Task<Result<PagedResult<OutputItemResponse>>> Handle(
        GetOutputsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable().Where(x => x.BuyerId == request.BuyerId);

        var result = await paginator.ApplyAsync<OutputEntity, OutputItemResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}
