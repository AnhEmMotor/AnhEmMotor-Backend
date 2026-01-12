using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed class GetDeletedOutputsListQueryHandler(IOutputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedOutputsListQuery, Result<PagedResult<OutputResponse>>>
{
    public async Task<Result<PagedResult<OutputResponse>>> Handle(
        GetDeletedOutputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return await paginator.ApplyAsync<OutputEntity, OutputResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken);
    }
}
