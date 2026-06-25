using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Entities;
using MediatR;

namespace Application.Features.DebtPayments.Queries.GetSupplierDebtLogs
{
    public class GetSupplierDebtLogsQueryHandler(ISupplierDebtReadRepository supplierDebtRepository) : IRequestHandler<GetSupplierDebtLogsQuery, Result<List<SupplierDebtLog>>>
    {
        public async Task<Result<List<SupplierDebtLog>>> Handle(
            GetSupplierDebtLogsQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await supplierDebtRepository.GetSupplierDebtLogsBySupplierIdAsync(
                request.SupplierId,
                cancellationToken);
            return Result<List<SupplierDebtLog>>.Success(logs);
        }
    }
}
