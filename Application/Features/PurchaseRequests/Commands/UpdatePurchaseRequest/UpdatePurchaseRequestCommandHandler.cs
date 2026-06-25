using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest
{
    public class UpdatePurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestUpdateRepository updateRepository,
        IPurchaseRequestInsertRepository insertRepository,
        IPurchaseRequestDeleteRepository deleteRepository,
        IProductVariantReadRepository variantRepository,
        ISupplierReadRepository supplierReadRepository,
        IPermissionReadRepository permissionReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdatePurchaseRequestCommand, Result<PurchaseRequestDetailResponse?>>
    {
        public async Task<Result<PurchaseRequestDetailResponse?>> Handle(
            UpdatePurchaseRequestCommand request,
            CancellationToken cancellationToken)
        {
            var pr = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (pr is null)
            {
                return Error.NotFound($"Không tìm thấy yêu cầu mua hàng có ID {request.Id}.", "Id");
            }
            if (string.Equals(pr.Status, "sent", StringComparison.OrdinalIgnoreCase))
            {
                var userId = currentUserContext.GetUserId();
                if (!await permissionReadRepository.CheckUserPermissionsAsync(
                    userId,
                    [Domain.Constants.Permission.Permissions.PurchaseRequests.ApproveReject],
                    cancellationToken)
                    .ConfigureAwait(false))
                {
                    return Error.BadRequest(
                        "Bạn không có quyền chỉnh sửa yêu cầu mua hàng khi đã ở trạng thái Sent.",
                        "Status");
                }
            } else if (string.Equals(pr.Status, "approve", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pr.Status, "reject", StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest(
                    "Không thể chỉnh sửa yêu cầu mua hàng đã được Phê duyệt hoặc Từ chối.",
                    "Status");
            }
            var variantIds = request.Items
                .Where(x => x.ProductVariantId.HasValue)
                .Select(x => x.ProductVariantId!.Value)
                .Distinct()
                .ToList();
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            var variantDict = variants.ToDictionary(v => v.Id);
            var supplierIds = request.Items
                .Where(x => x.SupplierId.HasValue)
                .Select(x => x.SupplierId!.Value)
                .Concat(pr.PurchaseRequestItems.Where(x => x.SupplierId.HasValue).Select(x => x.SupplierId!.Value))
                .Distinct()
                .ToList();
            var suppliers = await supplierReadRepository.GetByIdAsync(
                supplierIds,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            var supplierDict = suppliers.ToDictionary(s => s.Id, s => s.Name);
            foreach (var item in request.Items)
            {
                if (!item.ProductVariantId.HasValue)
                {
                    return Error.BadRequest("ProductVariantId không được để trống.", "Items");
                }
                if (!variantDict.TryGetValue(item.ProductVariantId.Value, out var variant))
                {
                    return Error.NotFound(
                        $"Không tìm thấy biến thể sản phẩm có ID {item.ProductVariantId.Value}.",
                        "Items");
                }
                var colors = variant.ProductVariantColors ?? [];
                if (colors.Count > 0)
                {
                    if (!item.ProductVariantColorId.HasValue)
                    {
                        return Error.BadRequest(
                            $"Sản phẩm '{variant.Product?.Name ?? variant.VariantName}' yêu cầu chọn màu sắc.",
                            "Items");
                    }
                    if (colors.All(c => c.Id != item.ProductVariantColorId.Value))
                    {
                        return Error.BadRequest(
                            $"Màu sắc đã chọn không thuộc sản phẩm '{variant.Product?.Name ?? variant.VariantName}'.",
                            "Items");
                    }
                } else
                {
                    if (item.ProductVariantColorId.HasValue)
                    {
                        return Error.BadRequest(
                            $"Sản phẩm '{variant.Product?.Name ?? variant.VariantName}' không hỗ trợ chọn màu sắc.",
                            "Items");
                    }
                }
                if (!item.Quantity.HasValue || item.Quantity.Value <= 0)
                {
                    return Error.BadRequest("Số lượng sản phẩm yêu cầu phải lớn hơn 0.", "Items");
                }
            }
            var currentUserId = currentUserContext.GetUserId();
            var oldNotes = pr.Note;
            pr.Note = request.Note;
            var auditLog = new PurchaseRequestAuditLog
            {
                PurchaseRequest = pr,
                Action = "Update",
                ChangedById = currentUserId,
                ChangedAt = DateTimeOffset.UtcNow,
                OldNotes = oldNotes,
                NewNotes = pr.Note
            };
            var auditLogs = new List<PurchaseRequestAuditLog> { auditLog };
            var existingItemsDict = pr.PurchaseRequestItems.ToDictionary(x => x.Id);
            var requestItemsDict = request.Items.Where(x => x.Id.HasValue && x.Id > 0).ToDictionary(x => x.Id!.Value);
            var toDelete = pr.PurchaseRequestItems.Where(x => !requestItemsDict.ContainsKey(x.Id)).ToList();
            var itemAuditLogs = new List<PurchaseRequestItemAuditLog>();
            foreach (var item in toDelete)
            {
                itemAuditLogs.Add(
                    new PurchaseRequestItemAuditLog
                    {
                        PurchaseRequestItem = item,
                        Action = "Delete",
                        OldQuantity = item.Quantity,
                        OldProductVariantId = item.ProductVariantId,
                        OldProductVariantColorId = item.ProductVariantColorId,
                        OldSupplierName =
                            item.SupplierId.HasValue &&
                                        supplierDict.TryGetValue(item.SupplierId.Value, out var supplierName)
                                    ? supplierName
                                    : null,
                        OldUnitPrice = item.UnitPrice
                    });
                deleteRepository.DeleteItem(item);
                pr.PurchaseRequestItems.Remove(item);
            }
            foreach (var itemRequest in request.Items)
            {
                if (itemRequest.Id.HasValue && itemRequest.Id > 0)
                {
                    if (existingItemsDict.TryGetValue(itemRequest.Id.Value, out var existingItem))
                    {
                        var oldQuantity = existingItem.Quantity;
                        var oldVariantId = existingItem.ProductVariantId;
                        var oldColorId = existingItem.ProductVariantColorId;
                        var oldUnitPrice = existingItem.UnitPrice;
                        existingItem.ProductVariantId = itemRequest.ProductVariantId!.Value;
                        existingItem.ProductVariantColorId = itemRequest.ProductVariantColorId;
                        existingItem.Quantity = itemRequest.Quantity!.Value;
                        existingItem.SupplierId = itemRequest.SupplierId;
                        existingItem.ProductQuotationId = itemRequest.ProductQuotationId;
                        existingItem.UnitPrice = itemRequest.UnitPrice;
                        if (oldQuantity != existingItem.Quantity ||
                            oldVariantId != existingItem.ProductVariantId ||
                            oldColorId != existingItem.ProductVariantColorId ||
                            oldUnitPrice != existingItem.UnitPrice)
                        {
                            itemAuditLogs.Add(
                                new PurchaseRequestItemAuditLog
                                {
                                    PurchaseRequestItem = existingItem,
                                    Action = "Update",
                                    OldQuantity = oldQuantity,
                                    NewQuantity = existingItem.Quantity,
                                    OldProductVariantId = oldVariantId,
                                    NewProductVariantId = existingItem.ProductVariantId,
                                    OldProductVariantColorId = oldColorId,
                                    NewProductVariantColorId = existingItem.ProductVariantColorId,
                                    OldSupplierName =
                                        existingItem.SupplierId.HasValue &&
                                                    supplierDict.TryGetValue(
                                                        existingItem.SupplierId.Value,
                                                        out var oldSupplierName)
                                                ? oldSupplierName
                                                : null,
                                    NewSupplierName =
                                        itemRequest.SupplierId.HasValue &&
                                                    supplierDict.TryGetValue(
                                                        itemRequest.SupplierId.Value,
                                                        out var newSupplierName)
                                                ? newSupplierName
                                                : null,
                                    OldUnitPrice = oldUnitPrice,
                                    NewUnitPrice = existingItem.UnitPrice
                                });
                        }
                    }
                } else
                {
                    var newItem = new PurchaseRequestItem
                    {
                        ProductVariantId = itemRequest.ProductVariantId!.Value,
                        ProductVariantColorId = itemRequest.ProductVariantColorId,
                        Quantity = itemRequest.Quantity!.Value,
                        SupplierId = itemRequest.SupplierId,
                        ProductQuotationId = itemRequest.ProductQuotationId,
                        UnitPrice = itemRequest.UnitPrice
                    };
                    pr.PurchaseRequestItems.Add(newItem);
                    itemAuditLogs.Add(
                        new PurchaseRequestItemAuditLog
                        {
                            PurchaseRequestItem = newItem,
                            Action = "Add",
                            NewQuantity = newItem.Quantity,
                            NewProductVariantId = newItem.ProductVariantId,
                            NewProductVariantColorId = newItem.ProductVariantColorId,
                            NewSupplierName =
                                newItem.SupplierId.HasValue &&
                                            supplierDict.TryGetValue(newItem.SupplierId.Value, out var nSupplierName)
                                        ? nSupplierName
                                        : null,
                            NewUnitPrice = newItem.UnitPrice
                        });
                }
            }
            updateRepository.Update(pr);
            await insertRepository.InsertAuditLogsAsync(auditLogs, cancellationToken).ConfigureAwait(false);
            await insertRepository.InsertItemAuditLogsAsync(itemAuditLogs, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var updated = await readRepository.GetByIdWithDetailsAsync(pr.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<PurchaseRequestDetailResponse?>();
        }
    }
}
