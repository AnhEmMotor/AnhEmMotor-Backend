using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId
{
    public sealed class GetReceiptsWithDebtBySupplierIdQueryHandler(IInventoryReceiptReadRepository inventoryReceiptReadRepository)
        : IRequestHandler<GetReceiptsWithDebtBySupplierIdQuery, Result<List<InventoryReceiptDebtLineResponse>>>
    {
        public async Task<Result<List<InventoryReceiptDebtLineResponse>>> Handle(
            GetReceiptsWithDebtBySupplierIdQuery request,
            CancellationToken cancellationToken)
        {
            var receipts = await inventoryReceiptReadRepository.GetBySupplierIdAsync(request.SupplierId, cancellationToken).ConfigureAwait(false);

            var result = new List<InventoryReceiptDebtLineResponse>();

            foreach (var receipt in receipts)
            {
                foreach (var ii in receipt.InventoryReceiptInfos)
                {
                    var lineSupplierId = ii.QuotationProductRow?.QuotationReceipt?.SupplierId;
                    if (lineSupplierId != request.SupplierId)
                    {
                        continue;
                    }

                    decimal totalAmount = (decimal)(ii.Count ?? 0) * (ii.QuotationProductRow?.QuotePrice ?? 0);
                    decimal paidAmount = ii.PaidAmount;

                    if (paidAmount < totalAmount)
                    {
                        var variant = ii.QuotationProductRow?.ProductVariant ?? ii.PurchaseRequestItem?.ProductVariant;
                        string? productVariantName = null;
                        if (variant != null && variant.Product != null)
                        {
                            var productName = variant.Product.Name ?? string.Empty;
                            productVariantName = string.IsNullOrWhiteSpace(variant.VariantName)
                                ? productName
                                : $"{productName} ({variant.VariantName})";
                        }

                        string? colorName = ii.QuotationProductRow?.ProductVariantColor?.ColorName 
                            ?? ii.PurchaseRequestItem?.ProductVariantColor?.ColorName;

                        result.Add(new InventoryReceiptDebtLineResponse
                        {
                            Id = ii.Id,
                            InventoryReceiptId = ii.InventoryReceiptId,
                            ProductVariantId = ii.QuotationProductRow?.ProductVariantId ?? ii.PurchaseRequestItem?.ProductVariantId,
                            ProductVariantColorId = ii.QuotationProductRow?.ProductVariantColorId ?? ii.PurchaseRequestItem?.ProductVariantColorId,
                            ProductVariantName = productVariantName,
                            ColorName = colorName,
                            TotalAmount = totalAmount,
                            PaidAmount = paidAmount
                        });
                    }
                }
            }

            return result;
        }
    }
}
