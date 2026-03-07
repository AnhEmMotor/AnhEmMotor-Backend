using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Features.Outputs.Queries.GetOutputsByUserId;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Primitives;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;

public sealed class GetOutputsByUserIdByManagerQueryHandler(IOutputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetOutputsByUserIdByManagerQuery, Result<PagedResult<OutputItemResponse>>>
{
    public async Task<Result<PagedResult<OutputItemResponse>>> Handle(
        GetOutputsByUserIdByManagerQuery request,
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
