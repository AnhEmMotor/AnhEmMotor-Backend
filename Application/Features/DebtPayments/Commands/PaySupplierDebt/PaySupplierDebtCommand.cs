using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Commands.PaySupplierDebt
{
    public class PaySupplierDebtCommand : IRequest<Result<bool>>
    {
        public int SupplierId { get; set; }
        public decimal Amount { get; set; }
    }
}
