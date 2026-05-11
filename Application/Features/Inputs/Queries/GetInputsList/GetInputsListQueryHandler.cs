using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Input;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputsList;

public sealed class GetInputsListQueryHandler(IInputReadRepository repository) : IRequestHandler<GetInputsListQuery, Result<PagedResult<InputListResponse>>>
{
    public async Task<Result<PagedResult<InputListResponse>>> Handle(
        GetInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InputListResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
