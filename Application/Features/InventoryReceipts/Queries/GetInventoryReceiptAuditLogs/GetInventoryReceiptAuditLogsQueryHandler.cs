using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants.InventoryReceipt;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptAuditLogs;

public class GetInventoryReceiptAuditLogsQueryHandler(IInventoryReceiptReadRepository repository)
    : IRequestHandler<GetInventoryReceiptAuditLogsQuery, Result<List<InventoryReceiptAuditLogResponse>>>
{
    public async Task<Result<List<InventoryReceiptAuditLogResponse>>> Handle(
        GetInventoryReceiptAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await repository.GetAuditLogsAsync(request.Id, cancellationToken);
        var infoLogs = await repository.GetInfoAuditLogsAsync(request.Id, cancellationToken);
        var vehicleLogs = await repository.GetVehicleAuditLogsAsync(request.Id, cancellationToken);

        var result = logs.Select(l => new InventoryReceiptAuditLogResponse
        {
            Id = l.Id,
            Action = InventoryReceiptAuditLogAction.Translate(l.Action),
            ChangedByFullName = l.ChangedBy?.FullName,
            ChangedAt = l.ChangedAt,
            OldStatusId = l.OldStatusId == l.NewStatusId ? null : TranslateStatus(l.OldStatusId),
            NewStatusId = l.OldStatusId == l.NewStatusId ? null : TranslateStatus(l.NewStatusId),
            OldNotes = l.OldNotes == l.NewNotes ? null : l.OldNotes,
            NewNotes = l.OldNotes == l.NewNotes ? null : l.NewNotes,
            // Since we don't have AuditLogId mapping for sub-logs (like Output logs issue above), 
            // we will group them globally like the Output logs did.
            InfoLogs = infoLogs
                .Where(il => Math.Abs((il.CreatedAt.GetValueOrDefault() - l.ChangedAt).TotalSeconds) <= 5)
                .Select(il => 
                {
                    var info = il.InventoryReceiptInfo;
                    var prItem = info?.PurchaseRequestItem;
                    var productVariantName = prItem?.ProductVariant != null ? $"{prItem.ProductVariant.Product?.Name}" : null;
                    if (prItem?.ProductVariant != null && !string.IsNullOrEmpty(prItem.ProductVariant.VariantName)) {
                        productVariantName += $" - {prItem.ProductVariant.VariantName}";
                    }
                    if (prItem?.ProductVariantColor != null && productVariantName != null) {
                        productVariantName += $" - {prItem.ProductVariantColor.ColorName}";
                    }

                    return new InventoryReceiptInfoAuditLogResponse
                    {
                        Action = InventoryReceiptAuditLogAction.Translate(il.Action),
                        OldQuantity = il.Action == "Delete" ? null : il.OldQuantity,
                        NewQuantity = il.Action == "Delete" ? null : il.NewQuantity,
                        ProductVariantName = productVariantName,
                        SupplierName = prItem?.Supplier?.Name
                    };
                }).ToList(),
            VehicleLogs = vehicleLogs
                .Where(vl => Math.Abs((vl.ChangedAt - l.ChangedAt).TotalSeconds) <= 5)
                .Select(vl => {
                    var info = vl.Vehicle.InventoryReceiptInfo;
                    var prItem = info?.PurchaseRequestItem;
                    var productVariantName = prItem?.ProductVariant != null ? $"{prItem.ProductVariant.Product?.Name}" : null;
                    if (prItem?.ProductVariant != null && !string.IsNullOrEmpty(prItem.ProductVariant.VariantName)) {
                        productVariantName += $" - {prItem.ProductVariant.VariantName}";
                    }
                    if (prItem?.ProductVariantColor != null && productVariantName != null) {
                        productVariantName += $" - {prItem.ProductVariantColor.ColorName}";
                    }

                    return new VehicleAuditLogResponse
                    {
                        Action = InventoryReceiptAuditLogAction.Translate(vl.Action),
                        ChangedByFullName = vl.ChangedBy?.FullName,
                        ChangedAt = vl.ChangedAt,
                        OldVinNumber = vl.OldVinNumber,
                        NewVinNumber = vl.NewVinNumber,
                        OldEngineNumber = vl.OldEngineNumber,
                        NewEngineNumber = vl.NewEngineNumber,
                        ProductVariantName = productVariantName
                    };
                }).ToList()
        }).ToList();

        return result;
    }

    private string? TranslateStatus(string? statusId)
    {
        return statusId?.ToLower() switch
        {
            InventoryReceiptStatus.Draft => "Nháp",
            InventoryReceiptStatus.Sent => "Chờ duyệt",
            InventoryReceiptStatus.Approve => "Đã duyệt",
            InventoryReceiptStatus.Reject => "Từ chối",
            _ => statusId
        };
    }
}
