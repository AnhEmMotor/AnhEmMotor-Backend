using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.DebtPayments.Queries.GetSupplierDebtLogs
{
    public class GetSupplierDebtLogsQuery : IRequest<Result<List<SupplierDebtLog>>>
    {
        public int SupplierId { get; set; }
    }
}
