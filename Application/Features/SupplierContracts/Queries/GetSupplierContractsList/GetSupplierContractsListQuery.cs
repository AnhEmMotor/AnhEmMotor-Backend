using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractsList;

public sealed record GetSupplierContractsListQuery : IRequest<Result<PagedResult<SupplierContractResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
