using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed class GetDeletedInputsListQueryHandler(IInputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedInputsListQuery, Domain.Primitives.PagedResult<InputResponse>>
{
    public Task<Domain.Primitives.PagedResult<InputResponse>> Handle(
        GetDeletedInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return paginator.ApplyAsync<InputEntity, InputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
