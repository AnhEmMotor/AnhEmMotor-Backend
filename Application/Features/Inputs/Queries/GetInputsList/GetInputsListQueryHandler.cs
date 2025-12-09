using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed class GetInputsListQueryHandler(IInputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetInputsListQuery, Domain.Primitives.PagedResult<InputResponse>>
{
    public Task<Domain.Primitives.PagedResult<InputResponse>> Handle(
        GetInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<InputEntity, InputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
