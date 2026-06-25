using Application.ApiContracts.FinanceContract.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.FinanceContracts.Queries.GetFinanceContractsList;

public sealed record GetFinanceContractsListQuery : IRequest<Result<PagedResult<FinanceContractDetailResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
