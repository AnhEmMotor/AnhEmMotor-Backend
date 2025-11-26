using Application.ApiContracts.Output;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Domain.Shared;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsList;

public sealed class GetOutputsListQueryHandler(
    IOutputReadRepository repository,
    IPaginator paginator) : IRequestHandler<GetOutputsListQuery, PagedResult<OutputResponse>>
{
    public Task<PagedResult<OutputResponse>> Handle(
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
