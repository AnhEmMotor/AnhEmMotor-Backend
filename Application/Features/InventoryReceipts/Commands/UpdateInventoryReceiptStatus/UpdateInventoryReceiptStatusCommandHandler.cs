using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Services;
using Domain.Constants;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using System.Collections.Generic;
using Application.Interfaces.Repositories.Supplier;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;

public sealed class UpdateInventoryReceiptStatusCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    ICurrentUserContext currentUserContext,
    IInventoryLedgerRepository ledgerRepository,
    ISupplierDebtRepository supplierDebtRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInventoryReceiptStatusCommand, Result<InventoryReceiptDetailResponse>>
{
    public async Task<Result<InventoryReceiptDetailResponse>> Handle(
        UpdateInventoryReceiptStatusCommand request,
        CancellationToken cancellationToken)
    {
        var InventoryReceipt = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (InventoryReceipt is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id");
        }
        if (!string.Equals(InventoryReceipt.StatusId, InventoryReceiptStatus.Sent, StringComparison.OrdinalIgnoreCase))
        {
            return Error.BadRequest("Chỉ có thể duyệt hoặc từ chối phiếu nhập đang ở trạng thái đã gửi (sent).", "StatusId");
        }
        if (!string.Equals(request.StatusId, InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.StatusId, InventoryReceiptStatus.Reject, StringComparison.OrdinalIgnoreCase))
        {
            return Error.BadRequest("Trạng thái mới phải là phê duyệt (approve) hoặc từ chối (reject).", "StatusId");
        }
        InventoryReceipt.StatusId = request.StatusId;
        if (string.Equals(request.StatusId, InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase))
        {
            var currentUserId = currentUserContext.GetUserId();
            InventoryReceipt.InventoryReceiptDate = DateTimeOffset.UtcNow;
            InventoryReceipt.ConfirmedBy = currentUserId;
            InventoryReceipt.ApprovedBy = currentUserId;
            InventoryReceipt.RejectedBy = null;

            // Group items by Supplier to record SupplierDebt
            var supplierDebtsDict = new Dictionary<int, decimal>();
            var supplierPaidDict = new Dictionary<int, decimal>();

            foreach (var info in InventoryReceipt.InventoryReceiptInfos)
            {
                int? variantId = info.QuotationProductRow != null
                    ? info.QuotationProductRow.ProductVariantId
                    : (info.PurchaseRequestItem != null ? info.PurchaseRequestItem.ProductVariantId : (int?)null);
                
                int? colorId = info.QuotationProductRow != null
                    ? info.QuotationProductRow.ProductVariantColorId
                    : (info.PurchaseRequestItem != null ? info.PurchaseRequestItem.ProductVariantColorId : (int?)null);

                if (variantId.HasValue)
                {
                    var lastLedger = await ledgerRepository.GetLastEntryAsync(variantId.Value, colorId, cancellationToken).ConfigureAwait(false);
                    int prevStock = lastLedger?.StockAfter ?? 0;
                    int importQty = info.Count ?? 0;
                    int stockAfter = prevStock + importQty;

                    string? supplierName = info.QuotationProductRow?.QuotationReceipt?.Supplier?.Name;

                    decimal unitPrice = info.QuotationProductRow?.QuotePrice ?? 0;
                    decimal totalAmount = importQty * unitPrice;

                    var ledger = new Domain.Entities.InventoryLedger
                    {
                        TransactionDate = DateTimeOffset.UtcNow,
                        DocumentCode = $"IR-{InventoryReceipt.Id}",
                        TransactionType = "Nhập kho",
                        ProductVariantId = variantId.Value,
                        ProductVariantColorId = colorId,
                        PartnerName = supplierName,
                        ImportQty = importQty,
                        ExportQty = 0,
                        UnitPrice = unitPrice,
                        TotalAmount = totalAmount,
                        StockAfter = stockAfter
                    };

                    await ledgerRepository.AddAsync(ledger, cancellationToken).ConfigureAwait(false);

                    int? supplierId = info.QuotationProductRow?.QuotationReceipt?.SupplierId;
                    if (supplierId.HasValue)
                    {
                        if (supplierDebtsDict.TryGetValue(supplierId.Value, out decimal existingAmount))
                        {
                            supplierDebtsDict[supplierId.Value] = existingAmount + totalAmount;
                            supplierPaidDict[supplierId.Value] = supplierPaidDict[supplierId.Value] + info.PaidAmount;
                        }
                        else
                        {
                            supplierDebtsDict[supplierId.Value] = totalAmount;
                            supplierPaidDict[supplierId.Value] = info.PaidAmount;
                        }
                    }
                }
            }

            foreach (var kvp in supplierDebtsDict)
            {
                var supplierDebt = new Domain.Entities.SupplierDebt
                {
                    InventoryReceiptId = InventoryReceipt.Id,
                    SupplierId = kvp.Key,
                    TotalAmount = kvp.Value,
                    PaidAmount = supplierPaidDict.TryGetValue(kvp.Key, out decimal paidVal) ? paidVal : 0
                };
                supplierDebtRepository.Add(supplierDebt);
            }
        }
        else if (string.Equals(request.StatusId, InventoryReceiptStatus.Reject, StringComparison.OrdinalIgnoreCase))
        {
            var currentUserId = currentUserContext.GetUserId();
            InventoryReceipt.RejectedBy = currentUserId;
            InventoryReceipt.ApprovedBy = null;
            InventoryReceipt.ConfirmedBy = null;
        }
        updateRepository.Update(InventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(InventoryReceipt.Id, cancellationToken)
            .ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(updated);
        return updated.Adapt<InventoryReceiptDetailResponse>();
    }
}
