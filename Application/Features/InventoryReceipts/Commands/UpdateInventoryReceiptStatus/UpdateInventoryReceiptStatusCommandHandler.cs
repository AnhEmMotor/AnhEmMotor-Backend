using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ProductQuotations;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
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
        IProductQuotationReadRepository quotationReadRepository, IProductQuotationUpdateRepository quotationUpdateRepository, IProductQuotationInsertRepository quotationInsertRepository,
        IUnitOfWork unitOfWork, IPublisher publisher) : IRequestHandler<UpdateInventoryReceiptStatusCommand, Result<InventoryReceiptDetailResponse>>
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
                    var prItem = info.PurchaseRequestItem;
                    if (prItem != null)
                    {
                        var importedQty = prItem.InventoryReceiptInfos
                            .Where(ii => ii.DeletedAt == null &&
                                         ii.InventoryReceiptId != receipt.Id &&
                                         ii.InventoryReceipt != null &&
                                         ii.InventoryReceipt.DeletedAt == null &&
                                         string.Equals(ii.InventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Approve, StringComparison.OrdinalIgnoreCase))
                            .Sum(ii => ii.Count ?? 0);

                        var remainingAllowed = prItem.Quantity - importedQty;
                        var currentQty = info.Count ?? 0;

                        if (currentQty > remainingAllowed)
                        {
                            var productName = prItem.ProductVariant?.Product?.Name ?? $"Biến thể #{prItem.ProductVariantId}";
                            return Error.BadRequest(
                                $"Không thể phê duyệt. Số lượng nhập ({currentQty}) cho sản phẩm '{productName}' vượt quá giới hạn còn lại ({remainingAllowed}).",
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
                     var variantId = info.PurchaseRequestItem?.ProductVariantId ?? 0;
                     var colorId = info.PurchaseRequestItem?.ProductVariantColorId;
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
                         PartnerName = info.Supplier?.Name,
                         ImportQty = importQty,
                         ExportQty = 0,
                         UnitPrice = info.UnitPrice ?? 0,
                         TotalAmount = importQty * (info.UnitPrice ?? 0),
                         StockAfter = newStock
                     };

                     await ledgerRepository.AddAsync(ledger, cancellationToken).ConfigureAwait(false);

                     // Add/update supplier quotation prices in product section
                     if (info.SupplierId.HasValue && info.UnitPrice.HasValue && variantId > 0)
                     {
                         var existingQuote = await quotationReadRepository.GetBySupplierAndVariantAsync(variantId, colorId, info.SupplierId.Value, cancellationToken)
                             .ConfigureAwait(false);

                         if (existingQuote != null)
                         {
                             if (existingQuote.QuotePrice != (int)info.UnitPrice.Value)
                             {
                                 existingQuote.QuotePrice = (int)info.UnitPrice.Value;
                                 quotationUpdateRepository.Update(existingQuote);
                             }
                         }
                         else
                         {
                             var newQuote = new ProductQuotation
                             {
                                 ProductVariantId = variantId,
                                 ProductVariantColorId = colorId,
                                 SupplierId = info.SupplierId.Value,
                                 QuotePrice = (int)info.UnitPrice.Value
                             };
                             await quotationInsertRepository.AddAsync(newQuote, cancellationToken).ConfigureAwait(false);
                         }
                     }
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

            // Cập nhật tồn kho thủ công (Explicit Call)
            var combos = new System.Collections.Generic.HashSet<(int VariantId, int? ColorId)>();
            foreach (var info in receipt.InventoryReceiptInfos)
            {
                if (info.PurchaseRequestItem != null)
                {
                    combos.Add((info.PurchaseRequestItem.ProductVariantId, info.PurchaseRequestItem.ProductVariantColorId));
                }
            }
            if (combos.Count > 0)
            {
                await publisher.Publish(new Application.Features.InventoryOnHand.Notifications.InventoryChangedNotification(combos), cancellationToken).ConfigureAwait(false);
            }

            var updated = await readRepository.GetByIdWithDetailsAsync(receipt.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<InventoryReceiptDetailResponse>();
        }
    }
}


