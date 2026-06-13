using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetDeletedSupplierContractsList;

public class GetDeletedSupplierContractsListQueryHandler(
ISupplierContractReadRepository repository) : IRequestHandler<GetDeletedSupplierContractsListQuery, Result<SupplierContractListResponse>>
{
public async Task<Result<SupplierContractListResponse>> Handle(GetDeletedSupplierContractsListQuery request, CancellationToken cancellationToken)
{
var result = await repository.GetPagedAsync<SupplierContractResponse>(request.SieveModel!, DataFetchMode.DeletedOnly, cancellationToken)
.ConfigureAwait(false);
return new SupplierContractListResponse(result.Items, result.TotalCount, result.PageNumber, result.PageSize);
}
}
