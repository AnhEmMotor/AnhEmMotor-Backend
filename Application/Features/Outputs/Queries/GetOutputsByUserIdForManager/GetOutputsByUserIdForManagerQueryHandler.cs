using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Output;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputsByUserIdForManager;

public sealed class GetOutputsByUserIdForManagerQueryHandler(IOutputReadRepository repository) : IRequestHandler<GetOutputsByUserIdForManagerQuery, Result<PagedResult<OutputItemResponse>>>
{
    public async Task<Result<PagedResult<OutputItemResponse>>> Handle(
        GetOutputsByUserIdForManagerQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<OutputItemResponse>(
            request.SieveModel!,
            DataFetchMode.ActiveOnly,
            x => x.BuyerId == request.BuyerId,
            cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}
