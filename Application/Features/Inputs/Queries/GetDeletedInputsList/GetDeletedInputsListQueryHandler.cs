using Application.ApiContracts.Input;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Enums;
using Domain.Shared;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed class GetDeletedInputsListQueryHandler(
    IInputReadRepository repository,
    IPaginator paginator) : IRequestHandler<GetDeletedInputsListQuery, PagedResult<InputResponse>>
{
    public Task<PagedResult<InputResponse>> Handle(
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
