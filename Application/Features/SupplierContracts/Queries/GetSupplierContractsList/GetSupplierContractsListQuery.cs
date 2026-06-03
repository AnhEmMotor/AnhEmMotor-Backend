using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using MediatR;
using Sieve.Models;
using Domain.Primitives;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractsList;

public sealed record GetSupplierContractsListQuery : IRequest<Result<PagedResult<SupplierContractResponse>>>
{
public SieveModel? SieveModel { get; init; }
}
