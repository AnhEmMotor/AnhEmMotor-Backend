using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.InventoryReceipt;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DebtPayments.Commands.RecordDebtPayment
{
    public sealed class RecordDebtPaymentCommandHandler(
        ISupplierDebtRepository supplierDebtRepository,
        IInventoryReceiptReadRepository inventoryReceiptReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<RecordDebtPaymentCommand, Result<InventoryReceiptDetailResponse>>
    {
        public async Task<Result<InventoryReceiptDetailResponse>> Handle(
            RecordDebtPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var debt = await supplierDebtRepository.GetByIdAsync(request.LineId, cancellationToken)
                .ConfigureAwait(false);

            if (debt == null)
            {
                return Error.NotFound($"Không tìm thấy công nợ nhà cung cấp với ID {request.LineId}.", "LineId");
            }

            var receipt = await inventoryReceiptReadRepository.GetByIdWithDetailsAsync(debt.InventoryReceiptId, cancellationToken)
                .ConfigureAwait(false);
            if (receipt != null && receipt.StatusId != Domain.Constants.InventoryReceiptStatus.Approve)
            {
                return Error.BadRequest("Phiếu nhập chưa được phê duyệt.", "StatusId");
            }

            decimal remainingDebt = debt.TotalAmount - debt.PaidAmount;
            decimal amountToPay = request.Amount ?? remainingDebt;

            if (amountToPay <= 0)
            {
                return Error.BadRequest("Số tiền thanh toán phải lớn hơn 0.", "Amount");
            }

            if (amountToPay > remainingDebt)
            {
                return Error.BadRequest($"Số tiền thanh toán ({amountToPay}) vượt quá dư nợ còn lại ({remainingDebt}).", "Amount");
            }

            debt.PaidAmount += amountToPay;
            supplierDebtRepository.Update(debt);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            int inventoryReceiptId = debt.InventoryReceiptId;

            if (inventoryReceiptId == 0)
            {
                return new InventoryReceiptDetailResponse();
            }

            var receiptDetail = await inventoryReceiptReadRepository.GetByIdWithDetailsAsync(inventoryReceiptId, cancellationToken)
                .ConfigureAwait(false);

            if (receiptDetail == null)
            {
                return new InventoryReceiptDetailResponse { Id = inventoryReceiptId };
            }

            return receiptDetail.Adapt<InventoryReceiptDetailResponse>();
        }
    }
}
