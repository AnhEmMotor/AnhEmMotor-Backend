using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.InventoryReceipt;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId
{
    public sealed class GetReceiptsWithDebtBySupplierIdQueryHandler(
        ISupplierDebtRepository supplierDebtRepository,
        IInventoryReceiptReadRepository inventoryReceiptReadRepository) 
        : IRequestHandler<GetReceiptsWithDebtBySupplierIdQuery, Result<List<InventoryReceiptDebtLineResponse>>>
    {
        public async Task<Result<List<InventoryReceiptDebtLineResponse>>> Handle(
            GetReceiptsWithDebtBySupplierIdQuery request,
            CancellationToken cancellationToken)
        {
            var debts = await supplierDebtRepository.GetBySupplierIdAsync(request.SupplierId, cancellationToken)
                .ConfigureAwait(false);

            var receipts = await inventoryReceiptReadRepository.GetAllAsync(cancellationToken)
                .ConfigureAwait(false);
            var receiptsList = receipts.ToList();

            var responseList = new List<InventoryReceiptDebtLineResponse>();

            foreach (var debt in debts)
            {
                var firstReceiptItem = debt.PurchaseInvoice?.PurchaseInvoiceItems?
                    .FirstOrDefault(i => i.InventoryReceiptInfo?.InventoryReceipt != null);

                int inventoryReceiptId = 0;
                if (firstReceiptItem != null && firstReceiptItem.InventoryReceiptInfo != null)
                {
                    inventoryReceiptId = firstReceiptItem.InventoryReceiptInfo.InventoryReceiptId;
                }
                else if (debt.PurchaseInvoice?.PurchaseOrderId.HasValue == true)
                {
                    var matchingReceipt = receiptsList.FirstOrDefault(r => r.PurchaseOrderId == debt.PurchaseInvoice.PurchaseOrderId);
                    if (matchingReceipt != null)
                    {
                        inventoryReceiptId = matchingReceipt.Id;
                    }
                }

                var firstItem = debt.PurchaseInvoice?.PurchaseInvoiceItems?.FirstOrDefault();

                var response = new InventoryReceiptDebtLineResponse
                {
                    Id = debt.Id,
                    InventoryReceiptId = inventoryReceiptId,
                    ProductVariantId = firstReceiptItem?.ProductVariantId ?? firstItem?.ProductVariantId,
                    ProductVariantColorId = firstReceiptItem?.ProductVariantColorId ?? firstItem?.ProductVariantColorId,
                    ProductVariantName = firstReceiptItem?.ProductVariant?.Product?.Name ?? firstItem?.ProductVariant?.Product?.Name ?? "Sản phẩm",
                    ColorName = firstReceiptItem?.ProductVariantColor?.ColorName ?? firstItem?.ProductVariantColor?.ColorName,
                    TotalAmount = debt.TotalAmount,
                    PaidAmount = debt.PaidAmount,
                    DueDate = debt.PurchaseInvoice?.DueDate
                };

                responseList.Add(response);
            }

            return responseList;
        }
    }
}
