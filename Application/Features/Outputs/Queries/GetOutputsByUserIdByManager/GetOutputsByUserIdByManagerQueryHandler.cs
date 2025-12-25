using Application.ApiContracts.Output.Responses;
using Application.Features.Outputs.Queries.GetOutputsByUserId;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using MediatR;
using OutputEntity = Domain.Entities.Output;

namespace Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;

public sealed class GetOutputsByUserIdByManagerQueryHandler(IOutputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetOutputsByUserIdQuery, Domain.Primitives.PagedResult<OutputResponse>>
{
    public Task<Domain.Primitives.PagedResult<OutputResponse>> Handle(
        GetOutputsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable().Where(o => o.BuyerId == request.BuyerId);

        return paginator.ApplyAsync<OutputEntity, OutputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
