using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Application.Interfaces.Repositories.Supplier;

namespace Application.Features.DebtPayments.Commands.RecordDebtPayment
{
    public sealed class RecordDebtPaymentCommandHandler(
        IInventoryReceiptReadRepository readRepository,
        ISupplierDebtRepository supplierDebtRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<RecordDebtPaymentCommand, Result<InventoryReceiptDetailResponse>>
    {
        public async Task<Result<InventoryReceiptDetailResponse>> Handle(
            RecordDebtPaymentCommand request,
            CancellationToken cancellationToken)
        {
            var line = await readRepository.GetInfoByIdAsync(request.LineId, cancellationToken).ConfigureAwait(false);
            if (line == null)
            {
                return Error.NotFound($"Không tìm thấy chi tiết phiếu nhập có ID {request.LineId}");
            }

            var receipt = line.InventoryReceiptReceipt;
            if (receipt == null)
            {
                return Error.NotFound($"Không tìm thấy phiếu nhập liên quan");
            }

            if (!string.Equals(receipt.StatusId, InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest("Chỉ đơn hàng đã được approve mới được thanh toán công nợ");
            }

            var supplierId = line.QuotationProductRow?.QuotationReceipt?.SupplierId;
            if (supplierId == null || supplierId == 0)
            {
                return Error.BadRequest("Dòng chi tiết phiếu nhập không có thông tin nhà cung cấp hợp lệ");
            }

            decimal totalAmount = (decimal)(line.Count ?? 0) * (line.QuotationProductRow?.QuotePrice ?? 0);
            decimal remainingDebt = totalAmount - line.PaidAmount;

            if (remainingDebt <= 0)
            {
                return Error.BadRequest("Khoản nợ của dòng chi tiết này đã được thanh toán đầy đủ");
            }

            decimal paymentAmount = request.Amount ?? remainingDebt;
            if (paymentAmount <= 0)
            {
                return Error.BadRequest("Số tiền thanh toán phải lớn hơn 0");
            }

            if (paymentAmount > remainingDebt)
            {
                return Error.BadRequest($"Số tiền thanh toán ({paymentAmount}) vượt quá số nợ còn lại của dòng này ({remainingDebt})");
            }

            line.PaidAmount += paymentAmount;

            var supplierDebt = receipt.SupplierDebts.FirstOrDefault(sd => sd.SupplierId == supplierId);
            if (supplierDebt != null)
            {
                supplierDebt.PaidAmount += paymentAmount;
                supplierDebtRepository.Update(supplierDebt);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updatedReceipt = await readRepository.GetByIdWithDetailsAsync(receipt.Id, cancellationToken).ConfigureAwait(false);
            if (updatedReceipt == null)
            {
                return Error.NotFound("Không tìm thấy phiếu nhập sau khi cập nhật");
            }
            return updatedReceipt.Adapt<InventoryReceiptDetailResponse>();
        }
    }
}
