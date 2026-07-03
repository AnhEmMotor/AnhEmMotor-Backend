using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierDebt;
using Mapster;
using MediatR;

namespace Application.Features.DebtPayments.Queries.GetSupplierDebtLogs
{
    public class GetSupplierDebtLogsQueryHandler(ISupplierDebtReadRepository supplierDebtRepository) : IRequestHandler<GetSupplierDebtLogsQuery, Result<List<SupplierDebtLogResponse>>>
    {
        public async Task<Result<List<SupplierDebtLogResponse>>> Handle(
            GetSupplierDebtLogsQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await supplierDebtRepository.GetSupplierDebtLogsBySupplierIdAsync(
                request.SupplierId,
                cancellationToken);
            var result = logs.Select(
                log =>
                {
                    var response = log.Adapt<SupplierDebtLogResponse>();
                    response.HasProofImage = log.ProofImages != null && log.ProofImages.Any();
                    return response;
                })
                .ToList();
            return Result<List<SupplierDebtLogResponse>>.Success(result);
        }
    }
}
