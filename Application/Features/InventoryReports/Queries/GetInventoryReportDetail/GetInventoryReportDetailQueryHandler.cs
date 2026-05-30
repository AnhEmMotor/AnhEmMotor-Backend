using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using Domain.Constants.Order;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportDetail
{
    public sealed class GetInventoryReportDetailQueryHandler(IProductReadRepository productRepository)
        : IRequestHandler<GetInventoryReportDetailQuery, Result<InventoryReportDetailResponse>>
    {
        public async Task<Result<InventoryReportDetailResponse>> Handle(
            GetInventoryReportDetailQuery request,
            CancellationToken cancellationToken)
        {
            var variant = await productRepository.GetVariantByIdWithDetailsAsync(request.VariantId, cancellationToken).ConfigureAwait(false);
            if (variant == null)
            {
                return Error.NotFound($"Không tìm thấy biến thể sản phẩm có ID {request.VariantId}");
            }

            var hasColors = variant.ProductVariantColors != null && variant.ProductVariantColors.Any();
            if (hasColors && request.ColorId == null)
            {
                return Error.BadRequest("Mã màu sắc (colorId) là bắt buộc đối với biến thể có nhiều màu sắc");
            }

            var imports = variant.InventoryReceiptInfos
                .Where(ii => ii.InventoryReceiptReceipt != null &&
                             InventoryReceiptStatus.IsFinished(ii.InventoryReceiptReceipt.StatusId))
                .Where(ii => !hasColors ||
                             (ii.QuotationProductRow != null && ii.QuotationProductRow.ProductVariantColorId == request.ColorId) ||
                             (ii.PurchaseRequestItem != null && ii.PurchaseRequestItem.ProductVariantColorId == request.ColorId))
                .Select(ii => new InventoryTransactionResponse
                {
                    PartnerName = ii.QuotationProductRow?.QuotationReceipt?.Supplier?.Name ??
                                  "Nhà cung cấp",
                    Qty = ii.Count ?? 0,
                    Price = ii.QuotationProductRow?.QuotePrice ?? 0,
                    Date = ii.InventoryReceiptReceipt.InventoryReceiptDate ?? ii.InventoryReceiptReceipt.CreatedAt ?? DateTimeOffset.MinValue
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            var exports = variant.OutputInfos
                .Where(oi => oi.OutputOrder != null &&
                             string.Equals(oi.OutputOrder.StatusId, OrderStatus.Completed, StringComparison.OrdinalIgnoreCase))
                .Where(oi => !hasColors || oi.ProductVariantColorId == request.ColorId)
                .Select(oi => new InventoryTransactionResponse
                {
                    PartnerName = oi.OutputOrder!.CustomerName ?? "Khách hàng",
                    Qty = oi.Count ?? 0,
                    Price = oi.Price ?? 0,
                    Date = oi.OutputOrder.LastStatusChangedAt ?? oi.OutputOrder.CreatedAt ?? DateTimeOffset.MinValue
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return new InventoryReportDetailResponse
            {
                Imports = imports,
                Exports = exports
            };
        }
    }
}
