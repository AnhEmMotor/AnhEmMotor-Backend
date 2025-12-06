using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Shared;
using MediatR;
using InputEntity = Domain.Entities.Input;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed class GetInputsListQueryHandler(IInputReadRepository repository, IPaginator paginator) : IRequestHandler<GetInputsListQuery, PagedResult<InputResponse>>
{
    public Task<PagedResult<InputResponse>> Handle(GetInputsListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<InputEntity, InputResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}
