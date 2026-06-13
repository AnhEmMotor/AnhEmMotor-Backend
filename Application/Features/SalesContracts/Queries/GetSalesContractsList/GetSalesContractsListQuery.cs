using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.SalesContracts.Queries.GetSalesContractsList;

public sealed record GetSalesContractsListQuery : IRequest<Result<PagedResult<SalesContractResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
