using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using MediatR;
using Sieve.Models;

namespace Application.Features.SupplierContracts.Queries.GetDeletedSupplierContractsList;

public sealed record GetDeletedSupplierContractsListQuery : IRequest<Result<SupplierContractListResponse>>
{
    public SieveModel? SieveModel { get; init; }
}
