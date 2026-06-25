using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Product;
using Domain.Constants.InventoryReceipt;
using Domain.Constants.Order;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportDetail
{
    public class GetInventoryReportDetailQueryHandler(
        IProductReadRepository productRepository,
        IInventoryReceiptReadRepository receiptRepository) : IRequestHandler<GetInventoryReportDetailQuery, Result<InventoryReportDetailResponse>>
    {
        public async Task<Result<InventoryReportDetailResponse>> Handle(
            GetInventoryReportDetailQuery request,
            CancellationToken cancellationToken)
        {
            var variant = await productRepository.GetVariantByIdWithDetailsAsync(request.VariantId, cancellationToken)
                .ConfigureAwait(false);
            if (variant == null)
            {
                return Error.NotFound($"Không tìm thấy biến thể sản phẩm có ID {request.VariantId}");
            }
            var hasColors = variant.ProductVariantColors != null && variant.ProductVariantColors.Any();
            if (hasColors && request.ColorId == null)
            {
                return Error.BadRequest("Mã màu sắc (colorId) là bắt buộc đối với biến thể có nhiều màu sắc");
            }
            var receiptInfos = await receiptRepository.GetInfosByVariantAsync(
                request.VariantId,
                request.ColorId,
                cancellationToken)
                .ConfigureAwait(false);
            var imports = receiptInfos
                .Where(
                    ii => ii.InventoryReceipt != null && InventoryReceiptStatus.IsFinished(ii.InventoryReceipt.StatusId))
                .Select(
                    ii => new InventoryTransactionResponse
                    {
                        Id = ii.InventoryReceipt!.Id,
                        PartnerName = ii.PurchaseRequestItem?.Supplier?.Name ?? "Nhà cung cấp",
                        Qty = ii.Count ?? 0,
                        Price = (int)(ii.PurchaseRequestItem?.UnitPrice ?? 0),
                        Date =
                            ii.InventoryReceipt!.InventoryReceiptDate ??
                                    ii.InventoryReceipt.CreatedAt ??
                                    DateTimeOffset.MinValue
                    })
                .OrderByDescending(x => x.Date)
                .ToList();
            var exports = variant.OutputInfos
                .Where(
                    oi => oi.OutputOrder != null &&
                        string.Equals(
                            oi.OutputOrder.StatusId,
                            OrderStatus.Completed,
                            StringComparison.OrdinalIgnoreCase))
                .Where(oi => !hasColors || oi.ProductVariantColorId == request.ColorId)
                .Select(
                    oi => new InventoryTransactionResponse
                    {
                        Id = oi.OutputOrder!.Id,
                        PartnerName = oi.OutputOrder!.CustomerName ?? "Khách hàng",
                        Qty = oi.Count ?? 0,
                        Price = oi.Price ?? 0,
                        Date = oi.OutputOrder.LastStatusChangedAt ?? oi.OutputOrder.CreatedAt ?? DateTimeOffset.MinValue
                    })
                .OrderByDescending(x => x.Date)
                .ToList();
            return new InventoryReportDetailResponse { Imports = imports, Exports = exports };
        }
    }
}
