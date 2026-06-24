using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Features.InventoryOnHand.Notifications;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ProductQuotations;
using Application.Interfaces.Repositories.SupplierDebt;
using Application.Interfaces.Services;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus
{
    public class UpdateInventoryReceiptStatusCommandHandler(
        IInventoryReceiptReadRepository readRepository,
        IInventoryReceiptUpdateRepository updateRepository,
        IInventoryReceiptInsertRepository insertRepository,
        ICurrentUserContext currentUserContext,
        IInventoryLedgerRepository ledgerRepository,
        ISupplierDebtInsertRepository supplierDebtInsertRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher) : IRequestHandler<UpdateInventoryReceiptStatusCommand, Result<InventoryReceiptDetailResponse>>
    {
        public async Task<Result<InventoryReceiptDetailResponse>> Handle(
            UpdateInventoryReceiptStatusCommand request,
            CancellationToken cancellationToken)
        {
            var receipt = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (receipt is null)
            {
                return Error.NotFound($"Không tìm thấy phiếu nhập kho với ID {request.Id}.", "Id");
            }
            if (string.Compare(receipt.StatusId, Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Sent) != 0)
            {
                return Error.BadRequest(
                    "Phiếu nhập kho phải ở trạng thái đã gửi (sent) mới có thể phê duyệt hoặc từ chối.",
                    "StatusId");
            }
            var currentUserId = currentUserContext.GetUserId();
            if (string.Equals(
                request.StatusId,
                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve,
                StringComparison.OrdinalIgnoreCase))
            {
                foreach (var info in receipt.InventoryReceiptInfos)
                {
                    var prItem = info.PurchaseRequestItem;
                    if (prItem != null)
                    {
                        var importedQty = prItem.InventoryReceiptInfos
                            .Where(
                                ii => ii.DeletedAt == null &&
                                    ii.InventoryReceiptId != receipt.Id &&
                                    ii.InventoryReceipt != null &&
                                    ii.InventoryReceipt.DeletedAt == null &&
                                    string.Equals(
                                        ii.InventoryReceipt.StatusId,
                                        Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve,
                                        StringComparison.OrdinalIgnoreCase))
                            .Sum(ii => ii.Count ?? 0);
                        var remainingAllowed = prItem.Quantity - importedQty;
                        var currentQty = info.Count ?? 0;
                        if (currentQty > remainingAllowed)
                        {
                            var productName = prItem.ProductVariant?.Product?.Name ??
                                $"Biến thể #{prItem.ProductVariantId}";
                            return Error.BadRequest(
                                $"Không thể phê duyệt. Số lượng nhập ({currentQty}) cho sản phẩm '{productName}' vượt quá giới hạn còn lại ({remainingAllowed}).",
                                "StatusId");
                        }
                    }
                }
                receipt.StatusId = Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve;
                receipt.ApprovedBy = currentUserId;
                receipt.ConfirmedBy = currentUserId;
                receipt.RejectedBy = null;
                foreach (var info in receipt.InventoryReceiptInfos)
                {
                    var variantId = info.PurchaseRequestItem?.ProductVariantId ?? 0;
                    var colorId = info.PurchaseRequestItem?.ProductVariantColorId;
                    var lastEntry = await ledgerRepository.GetLastEntryAsync(variantId, colorId, cancellationToken)
                        .ConfigureAwait(false);
                    var currentStock = lastEntry?.StockAfter ?? 0;
                    var importQty = info.Count ?? 0;
                    var newStock = currentStock + importQty;
                    var ledger = new InventoryLedger
                    {
                        TransactionDate = DateTimeOffset.UtcNow,
                        DocumentCode = $"IR-{receipt.Id}",
                        TransactionType = "Nhập kho",
                        ProductVariantId = variantId,
                        ProductVariantColorId = colorId,
                        PartnerName = info.PurchaseRequestItem?.Supplier?.Name,
                        ImportQty = importQty,
                        ExportQty = 0,
                        UnitPrice = info.PurchaseRequestItem?.UnitPrice ?? 0,
                        TotalAmount = importQty * (info.PurchaseRequestItem?.UnitPrice ?? 0),
                        StockAfter = newStock
                    };
                    await ledgerRepository.AddAsync(ledger, cancellationToken).ConfigureAwait(false);
                }
                var debtsToCreate = receipt.InventoryReceiptInfos
                    .Where(info => info.PurchaseRequestItem?.SupplierId != null && info.Count.HasValue && info.Count.Value > 0)
                    .GroupBy(info => info.PurchaseRequestItem!.SupplierId!.Value)
                    .Select(
                        g => new { SupplierId = g.Key, TotalAmount = g.Sum(i => (i.Count ?? 0) * (i.PurchaseRequestItem?.UnitPrice ?? 0)) })
                    .Where(x => x.TotalAmount > 0)
                    .ToList();
                foreach (var debtInfo in debtsToCreate)
                {
                    var debt = new SupplierDebt
                    {
                        InventoryReceiptId = receipt.Id,
                        SupplierId = debtInfo.SupplierId,
                        TotalAmount = debtInfo.TotalAmount,
                        PaidAmount = 0
                    };
                    supplierDebtInsertRepository.Add(debt);
                }
            } else if (string.Equals(
                       request.StatusId,
                       Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Reject,
                       StringComparison.OrdinalIgnoreCase))
            {
                receipt.StatusId = Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Reject;
                receipt.RejectedBy = currentUserId;
                receipt.ApprovedBy = null;
                receipt.ConfirmedBy = null;
            } else
            {
                return Error.BadRequest("Trạng thái phê duyệt không hợp lệ.", "StatusId");
            }
            updateRepository.Update(receipt);

            var receiptAuditLogs = new List<Domain.Entities.InventoryReceiptAuditLog>
            {
                new Domain.Entities.InventoryReceiptAuditLog
                {
                    InventoryReceipt = receipt,
                    Action = "Update",
                    ChangedById = currentUserId,
                    ChangedAt = DateTimeOffset.UtcNow,
                    OldStatusId = receipt.StatusId,
                    NewStatusId = request.StatusId,
                    OldNotes = receipt.Notes,
                    NewNotes = receipt.Notes
                }
            };
            
            await insertRepository.InsertAuditLogsAsync(receiptAuditLogs, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var combos = new HashSet<(int VariantId, int? ColorId)>();
            foreach (var info in receipt.InventoryReceiptInfos)
            {
                if (info.PurchaseRequestItem != null)
                {
                    combos.Add(
                        (info.PurchaseRequestItem.ProductVariantId, info.PurchaseRequestItem.ProductVariantColorId));
                }
            }
            if (combos.Count > 0)
            {
                await publisher.Publish(new InventoryChangedNotification(combos), cancellationToken)
                    .ConfigureAwait(false);
            }
            var updated = await readRepository.GetByIdWithDetailsAsync(receipt.Id, cancellationToken)
                .ConfigureAwait(false);
            return updated!.Adapt<InventoryReceiptDetailResponse>();
        }
    }
}

