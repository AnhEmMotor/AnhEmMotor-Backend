using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Outputs.Queries.GetDeletedOutputsList;

public class GetDeletedOutputsListQueryHandler(IOutputReadRepository repository) : IRequestHandler<GetDeletedOutputsListQuery, Result<PagedResult<OutputItemResponse>>>
{
    public async Task<Result<PagedResult<OutputItemResponse>>> Handle(
        GetDeletedOutputsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<OutputItemResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
