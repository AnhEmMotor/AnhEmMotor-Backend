using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Shared;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed class GetDeletedOutputsListQueryHandler(IOutputReadRepository repository, IPaginator paginator) : IRequestHandler<GetDeletedOutputsListQuery, PagedResult<OutputResponse>>
{
    public Task<PagedResult<OutputResponse>> Handle(
        GetDeletedOutputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return paginator.ApplyAsync<OutputEntity, OutputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
