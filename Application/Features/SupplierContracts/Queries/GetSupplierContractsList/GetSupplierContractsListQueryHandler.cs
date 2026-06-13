using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Primitives;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractsList;

public class GetSupplierContractsListQueryHandler(
    ISupplierContractReadRepository repository) : IRequestHandler<GetSupplierContractsListQuery, Result<PagedResult<SupplierContractResponse>>>
{
    public async Task<Result<PagedResult<SupplierContractResponse>>> Handle(
        GetSupplierContractsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<SupplierContractResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
