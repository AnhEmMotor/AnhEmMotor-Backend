using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierDebt;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId
{
    public class GetReceiptsWithDebtBySupplierIdQueryHandler(ISupplierDebtReadRepository supplierDebtRepository) : IRequestHandler<GetReceiptsWithDebtBySupplierIdQuery, Result<List<InventoryReceiptDebtLineResponse>>>
    {
        public async Task<Result<List<InventoryReceiptDebtLineResponse>>> Handle(
            GetReceiptsWithDebtBySupplierIdQuery request,
            CancellationToken cancellationToken)
        {
            var debts = await supplierDebtRepository.GetBySupplierIdAsync(request.SupplierId, cancellationToken)
                .ConfigureAwait(false);
            var responseList = new List<InventoryReceiptDebtLineResponse>();
            foreach (var debt in debts)
            {
                var receipt = debt.InventoryReceipt;
                var firstInfo = receipt?.InventoryReceiptInfos?.FirstOrDefault();
                var response = new InventoryReceiptDebtLineResponse
                {
                    Id = debt.Id,
                    InventoryReceiptId = debt.InventoryReceiptId,
                    ProductVariantId = firstInfo?.PurchaseRequestItem?.ProductVariantId,
                    ProductVariantColorId = firstInfo?.PurchaseRequestItem?.ProductVariantColorId,
                    ProductVariantName = firstInfo?.PurchaseRequestItem?.ProductVariant?.Product?.Name ?? "Sản phẩm",
                    ColorName = firstInfo?.PurchaseRequestItem?.ProductVariantColor?.ColorName,
                    TotalAmount = debt.TotalAmount,
                    PaidAmount = debt.PaidAmount,
                    DueDate = receipt?.InventoryReceiptDate
                };
                responseList.Add(response);
            }
            return responseList;
        }
    }
}
