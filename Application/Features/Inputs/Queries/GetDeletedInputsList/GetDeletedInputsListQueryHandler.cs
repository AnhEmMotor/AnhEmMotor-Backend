using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Inputs.Queries.GetDeletedInputsList;

public sealed class GetDeletedInputsListQueryHandler(IInputReadRepository repository) : IRequestHandler<GetDeletedInputsListQuery, Result<PagedResult<InputListResponse>>>
{
    public async Task<Result<PagedResult<InputListResponse>>> Handle(
        GetDeletedInputsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<InputListResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
