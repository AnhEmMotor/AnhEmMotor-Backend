using Application.ApiContracts.Output.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsList;

public sealed class GetOutputsListQueryHandler(IOutputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetOutputsListQuery, Domain.Primitives.PagedResult<OutputResponse>>
{
    public Task<Domain.Primitives.PagedResult<OutputResponse>> Handle(
        GetOutputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<OutputEntity, OutputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
