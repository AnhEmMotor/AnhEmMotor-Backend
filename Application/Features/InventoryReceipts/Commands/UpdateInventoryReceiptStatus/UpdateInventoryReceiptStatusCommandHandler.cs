using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus
{
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
            var receipt = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (receipt is null)
            {
                return Error.NotFound($"Không tìm thấy phiếu nhập kho với ID {request.Id}.", "Id");
            }

            if (receipt.StatusId != Domain.Constants.InventoryReceiptStatus.Sent)
            {
                return Error.BadRequest("Phiếu nhập kho phải ở trạng thái đã gửi (sent) mới có thể phê duyệt hoặc từ chối.", "StatusId");
            }

            var currentUserId = currentUserContext.GetUserId();

            if (string.Equals(request.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var info in receipt.InventoryReceiptInfos)
                {
                    var poItem = info.PurchaseOrderItem;
                    if (poItem != null)
                    {
                        var importedQty = poItem.InventoryReceiptInfos
                            .Where(ii => ii.DeletedAt == null &&
                                         ii.InventoryReceiptId != receipt.Id &&
                                         ii.InventoryReceipt != null &&
                                         ii.InventoryReceipt.DeletedAt == null &&
                                         string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase))
                            .Sum(ii => ii.Count ?? 0);

                        var remainingAllowed = poItem.OrderedQuantity - importedQty;
                        var currentQty = info.Count ?? 0;

                        if (currentQty > remainingAllowed)
                        {
                            var productName = poItem.ProductVariant?.Product?.Name ?? $"Biến thể #{poItem.ProductVariantId}";
                            return Error.BadRequest(
                                $"Không thể phê duyệt. Số lượng nhập ({currentQty}) cho sản phẩm '{productName}' vượt quá giới hạn còn lại của PO ({remainingAllowed}).",
                                "StatusId");
                        }
                    }
                }

                receipt.StatusId = Domain.Constants.InventoryReceiptStatus.Approve;
                receipt.ApprovedBy = currentUserId;
                receipt.ConfirmedBy = currentUserId;
                receipt.RejectedBy = null;

                foreach (var info in receipt.InventoryReceiptInfos)
                 {
                     var variantId = info.PurchaseOrderItem?.ProductVariantId ?? 0;
                     var colorId = info.PurchaseOrderItem?.ProductVariantColorId;
                     var lastEntry = await ledgerRepository.GetLastEntryAsync(variantId, colorId, cancellationToken).ConfigureAwait(false);
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
                         PartnerName = receipt.PurchaseOrder?.Supplier?.Name,
                         ImportQty = importQty,
                         ExportQty = 0,
                         UnitPrice = info.PurchaseOrderItem?.UnitPrice ?? 0,
                         TotalAmount = importQty * (info.PurchaseOrderItem?.UnitPrice ?? 0),
                         StockAfter = newStock
                     };

                     await ledgerRepository.AddAsync(ledger, cancellationToken).ConfigureAwait(false);
                 }
            }
            else if (string.Equals(request.StatusId, Domain.Constants.InventoryReceiptStatus.Reject, StringComparison.OrdinalIgnoreCase))
            {
                receipt.StatusId = Domain.Constants.InventoryReceiptStatus.Reject;
                receipt.RejectedBy = currentUserId;
                receipt.ApprovedBy = null;
                receipt.ConfirmedBy = null;
            }
            else
            {
                return Error.BadRequest("Trạng thái phê duyệt không hợp lệ.", "StatusId");
            }

            updateRepository.Update(receipt);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(receipt.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<InventoryReceiptDetailResponse>();
        }
    }
}

