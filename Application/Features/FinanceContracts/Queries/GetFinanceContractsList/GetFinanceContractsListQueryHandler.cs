using Application.ApiContracts.FinanceContract.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.FinanceContract;
using Domain.Primitives;
using MediatR;

namespace Application.Features.FinanceContracts.Queries.GetFinanceContractsList;

public sealed class GetFinanceContractsListQueryHandler(
    IFinanceContractReadRepository repository
) : IRequestHandler<GetFinanceContractsListQuery, Result<PagedResult<FinanceContractDetailResponse>>>
{
    public async Task<Result<PagedResult<FinanceContractDetailResponse>>> Handle(
        GetFinanceContractsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository
            .GetPagedAsync<FinanceContractDetailResponse>(
                request.SieveModel ?? new(),
                cancellationToken: cancellationToken);
        return Result<PagedResult<FinanceContractDetailResponse>>.Success(result);
    }
}
