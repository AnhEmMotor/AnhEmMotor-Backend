using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using Domain.Constants.PurchaseRequest;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequestAuditLogs
{
    public class GetPurchaseRequestAuditLogsQueryHandler(
        IPurchaseRequestReadRepository readRepository,
        IProductVariantReadRepository variantRepository) : IRequestHandler<GetPurchaseRequestAuditLogsQuery, Result<List<PurchaseRequestAuditLogResponse>>>
    {
        public async Task<Result<List<PurchaseRequestAuditLogResponse>>> Handle(
            GetPurchaseRequestAuditLogsQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await readRepository.GetAuditLogsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            var pr = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            var itemIds = await readRepository.GetAllItemIdsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            var itemLogs = await readRepository.GetItemAuditLogsAsync(itemIds, cancellationToken).ConfigureAwait(false);
            var variantIds = itemLogs
                .SelectMany(il => new[] { il.OldProductVariantId, il.NewProductVariantId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            var variants = variantIds.Any()
                ? await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.All)
                    .ConfigureAwait(false)
                : new List<ProductVariant>();
            string? GetVariantFullName(int? variantId, int? colorId)
            {
                if (!variantId.HasValue)
                    return null;
                var variant = variants.FirstOrDefault(v => v.Id == variantId.Value);
                if (variant == null)
                    return null;
                var productName = variant.Product?.Name ?? string.Empty;
                var variantName = string.IsNullOrEmpty(variant.VariantName) ? string.Empty : $" - {variant.VariantName}";
                var colorName = string.Empty;
                if (colorId.HasValue)
                {
                    var color = variant.ProductVariantColors?.FirstOrDefault(c => c.Id == colorId.Value);
                    if (color != null && !string.IsNullOrEmpty(color.ColorName))
                    {
                        colorName = $" - {color.ColorName}";
                    }
                }
                return $"{productName}{variantName}{colorName}";
            }

            var result = logs.Select(
                l => new PurchaseRequestAuditLogResponse
                {
                    Id = l.Id,
                    PurchaseRequestId = l.PurchaseRequestId,
                    Action = PurchaseRequestAuditLogAction.Translate(l.Action),
                    ChangedById = l.ChangedById,
                    ChangedByName = l.ChangedBy?.FullName,
                    ChangedAt = l.ChangedAt,
                    OldStatusId = l.OldStatusId == l.NewStatusId ? null : TranslateStatus(l.OldStatusId),
                    NewStatusId = l.OldStatusId == l.NewStatusId ? null : TranslateStatus(l.NewStatusId),
                    OldNotes = l.OldNotes == l.NewNotes ? null : l.OldNotes,
                    NewNotes = l.OldNotes == l.NewNotes ? null : l.NewNotes,
                    ItemLogs =
                        itemLogs
                    .Where(il => Math.Abs((il.CreatedAt.GetValueOrDefault() - l.ChangedAt).TotalSeconds) <= 5)
                                .Select(
                                    il => new PurchaseRequestItemAuditLogResponse
                                {
                                    Id = il.Id,
                                    PurchaseRequestItemId = il.PurchaseRequestItemId,
                                    Action = PurchaseRequestAuditLogAction.Translate(il.Action),
                                    OldQuantity = il.Action == "Delete" ? null : il.OldQuantity,
                                    NewQuantity = il.Action == "Delete" ? null : il.NewQuantity,
                                    OldProductVariantId = il.OldProductVariantId,
                                    NewProductVariantId = il.NewProductVariantId,
                                    OldProductVariantName =
                                        GetVariantFullName(il.OldProductVariantId, il.OldProductVariantColorId),
                                    NewProductVariantName =
                                        GetVariantFullName(il.NewProductVariantId, il.NewProductVariantColorId),
                                    OldProductVariantColorId = il.OldProductVariantColorId,
                                    NewProductVariantColorId = il.NewProductVariantColorId,
                                    OldProductVariantColorName = il.PurchaseRequestItem?.ProductVariantColor?.ColorName,
                                    NewProductVariantColorName = il.PurchaseRequestItem?.ProductVariantColor?.ColorName,
                                    OldSupplierName = il.OldSupplierName,
                                    NewSupplierName = il.NewSupplierName,
                                    OldUnitPrice = il.OldUnitPrice,
                                    NewUnitPrice = il.NewUnitPrice,
                                    CreatedAt = il.CreatedAt.GetValueOrDefault()
                                })
                                .ToList()
                })
                .ToList();
            return result;
        }

        private string? TranslateStatus(string? statusId)
        {
            return statusId?.ToLower() switch
            {
                PurchaseRequestStatus.Draft => "Nháp",
                PurchaseRequestStatus.Sent => "Chờ duyệt",
                PurchaseRequestStatus.Approve => "Đã duyệt",
                PurchaseRequestStatus.Reject => "Từ chối",
                _ => statusId
            };
        }
    }
}
