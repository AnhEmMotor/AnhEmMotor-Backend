using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Primitives;
using MediatR;

namespace Application.Features.SalesContracts.Queries.GetSalesContractsList;

public class GetSalesContractsListQueryHandler(ISalesContractReadRepository readRepo) : IRequestHandler<GetSalesContractsListQuery, Result<PagedResult<SalesContractResponse>>>
{
    public async Task<Result<PagedResult<SalesContractResponse>>> Handle(
        GetSalesContractsListQuery request,
        CancellationToken cancellationToken)
    {
        var paged = await readRepo.GetPagedAsync(request.SieveModel ?? new(), cancellationToken).ConfigureAwait(false);
        return Result<PagedResult<SalesContractResponse>>.Success(paged);
    }
}
