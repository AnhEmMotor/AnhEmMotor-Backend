using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed class GetDeletedInputsListQueryHandler(IInputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedInputsListQuery, Result<PagedResult<InputResponse>>>
{
    public async Task<Result<PagedResult<InputResponse>>> Handle(
        GetDeletedInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return await paginator.ApplyAsync<InputEntity, InputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
