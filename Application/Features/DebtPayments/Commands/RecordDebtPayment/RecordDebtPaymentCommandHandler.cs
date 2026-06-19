using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.SupplierDebt;
using Application.Interfaces.Services;
using Domain.Constants.InventoryReceipt;
using Mapster;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.DebtPayments.Commands.RecordDebtPayment
{
    public class RecordDebtPaymentCommandHandler(
        ISupplierDebtReadRepository supplierDebtReadRepository,
        ISupplierDebtUpdateRepository supplierDebtUpdateRepository,
        IInventoryReceiptReadRepository inventoryReceiptReadRepository,
        ISupplierDebtAuditLogInsertRepository auditLogInsertRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<RecordDebtPaymentCommand, Result<InventoryReceiptDetailResponse>>
    {
        public async Task<Result<InventoryReceiptDetailResponse>> Handle(
            RecordDebtPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var debt = await supplierDebtReadRepository.GetByIdAsync(request.LineId, cancellationToken)
                .ConfigureAwait(false);
            if (debt == null)
            {
                return Error.NotFound($"Không tìm thấy công nợ nhà cung cấp với ID {request.LineId}.", "LineId");
            }
            var receipt = await inventoryReceiptReadRepository.GetByIdWithDetailsAsync(
                debt.InventoryReceiptId,
                cancellationToken)
                .ConfigureAwait(false);
            if (receipt != null && string.Compare(receipt.StatusId, InventoryReceiptStatus.Approve) != 0)
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
                return Error.BadRequest(
                    $"Số tiền thanh toán ({amountToPay}) vượt quá dư nợ còn lại ({remainingDebt}).",
                    "Amount");
            }
            decimal oldPaidAmount = debt.PaidAmount;
            debt.PaidAmount += amountToPay;
            supplierDebtUpdateRepository.Update(debt);

            var auditLog = new Domain.Entities.SupplierDebtAuditLog
            {
                SupplierDebtId = debt.Id,
                Action = "Thanh toán công nợ",
                OldAmount = oldPaidAmount,
                NewAmount = debt.PaidAmount,
                ChangedById = currentUserContext.GetUserId(),
                ChangedAt = DateTimeOffset.UtcNow,
                NewPaymentDate = DateTimeOffset.UtcNow
            };
            await auditLogInsertRepository.InsertAsync(auditLog, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            int inventoryReceiptId = debt.InventoryReceiptId;
            if (inventoryReceiptId == 0)
            {
                return new InventoryReceiptDetailResponse();
            }
            var receiptDetail = await inventoryReceiptReadRepository.GetByIdWithDetailsAsync(
                inventoryReceiptId,
                cancellationToken)
                .ConfigureAwait(false);
            if (receiptDetail == null)
            {
                return new InventoryReceiptDetailResponse { Id = inventoryReceiptId };
            }
            return receiptDetail.Adapt<InventoryReceiptDetailResponse>();
        }
    }
}
