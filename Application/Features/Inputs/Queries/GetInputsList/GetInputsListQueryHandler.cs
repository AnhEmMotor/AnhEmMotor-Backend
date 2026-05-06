using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Primitives;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed class GetInputsListQueryHandler(IInputReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetInputsListQuery, Result<PagedResult<InputListResponse>>>
{
    public async Task<Result<PagedResult<InputListResponse>>> Handle(
        GetInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();
        var result = await paginator.ApplyAsync<InputEntity, InputListResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
