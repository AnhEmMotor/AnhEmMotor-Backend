using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DebtPayments.Commands.RecordDebtPayment
{
    public sealed class RecordDebtPaymentCommandHandler : IRequestHandler<RecordDebtPaymentCommand, Result<InventoryReceiptDetailResponse>>
    {
        public Task<Result<InventoryReceiptDetailResponse>> Handle(
            RecordDebtPaymentCommand request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
