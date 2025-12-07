using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public sealed class GetDeletedOutputsListQueryHandler(IOutputReadRepository repository, IPaginator paginator) : IRequestHandler<GetDeletedOutputsListQuery, Domain.Primitives.PagedResult<OutputResponse>>
{
    public Task<Domain.Primitives.PagedResult<OutputResponse>> Handle(
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
