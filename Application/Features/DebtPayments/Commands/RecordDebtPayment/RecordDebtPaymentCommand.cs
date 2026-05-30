using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Commands.RecordDebtPayment
{
    public class RecordDebtPaymentCommand : IRequest<Result<InventoryReceiptDetailResponse>>
    {
        public int LineId { get; set; }
        public decimal? Amount { get; set; }
    }
}
