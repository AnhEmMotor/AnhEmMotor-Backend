using Application.ApiContracts.SalesContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SalesContract;
using Mapster;
using MediatR;

namespace Application.Features.SalesContracts.Queries.GetSalesContractById;

public sealed class GetSalesContractByIdQueryHandler(
    ISalesContractReadRepository readRepo) : IRequestHandler<GetSalesContractByIdQuery, Result<SalesContractResponse>>
{
    public async Task<Result<SalesContractResponse>> Handle(
        GetSalesContractByIdQuery request,
        CancellationToken cancellationToken)
    {
        var contract = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (contract == null)
            return Result<SalesContractResponse>.Failure("Không tìm thấy hợp đồng.");

        return Result<SalesContractResponse>.Success(contract.Adapt<SalesContractResponse>());
    }
}
