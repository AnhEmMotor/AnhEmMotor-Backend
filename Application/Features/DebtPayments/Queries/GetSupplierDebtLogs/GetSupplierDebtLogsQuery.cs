using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Queries.GetSupplierDebtLogs
{
    public class GetSupplierDebtLogsQuery : IRequest<Result<List<SupplierDebtLogResponse>>>
    {
        public int SupplierId { get; set; }
    }
}
