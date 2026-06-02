using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseOrder;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Permission.Permissions;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

namespace Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder
{
    public sealed class UpdatePurchaseOrderCommandHandler(
        IPurchaseOrderReadRepository readRepository,
        IPurchaseOrderUpdateRepository updateRepository,
        IPurchaseOrderDeleteRepository deleteRepository,
        IPurchaseOrderInsertRepository insertRepository,
        IProductVariantReadRepository variantRepository,
        IPermissionReadRepository permissionReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdatePurchaseOrderCommand, Result<PurchaseOrderDetailResponse?>>
    {
        public async Task<Result<PurchaseOrderDetailResponse?>> Handle(
            UpdatePurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            var po = await readRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (po is null)
            {
                return Error.NotFound($"Không tìm thấy đơn chốt mua có ID {request.Id}.", "Id");
            }

            var userId = currentUserContext.GetUserId();

            if (string.Equals(po.Status, PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase))
            {
                if (!await permissionReadRepository.CheckUserPermissionsAsync(
                    userId,
                    [Domain.Constants.Permission.Permissions.InventoryReceipts.Edit],
                    cancellationToken)
                    .ConfigureAwait(false))
                {
                    return Error.Forbidden("Bạn không có quyền chỉnh sửa đơn chốt mua.", "Permission");
                }
            }
            else if (string.Equals(po.Status, PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase))
            {
                var hasEdit = await permissionReadRepository.CheckUserPermissionsAsync(userId, [Domain.Constants.Permission.Permissions.InventoryReceipts.Edit], cancellationToken).ConfigureAwait(false);
                var hasApproveReject = await permissionReadRepository.CheckUserPermissionsAsync(userId, [Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject], cancellationToken).ConfigureAwait(false);
                if (!hasEdit || !hasApproveReject)
                {
                    return Error.Forbidden("Bạn cần có cả quyền Edit và Approve/Reject để chỉnh sửa đơn chốt mua ở trạng thái Sent.", "Permission");
                }
            }
            else
            {
                return Error.BadRequest("Không thể chỉnh sửa đơn chốt mua ở trạng thái hiện tại.", "Status");
            }

            var variantIds = request.Items
                .Where(x => x.ProductVariantId.HasValue)
                .Select(x => x.ProductVariantId!.Value)
                .Distinct()
                .ToList();
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            var variantDict = variants.ToDictionary(v => v.Id);

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
                }
                else
                {
                    if (item.ProductVariantColorId.HasValue)
                    {
                        return Error.BadRequest(
                            $"Sản phẩm '{variant.Product?.Name ?? variant.VariantName}' không hỗ trợ chọn màu sắc.",
                            "Items");
                    }
                }

                if (!item.OrderedQuantity.HasValue || item.OrderedQuantity.Value <= 0)
                {
                    return Error.BadRequest("Số lượng sản phẩm chốt mua phải lớn hơn 0.", "Items");
                }

                if (!item.UnitPrice.HasValue || item.UnitPrice.Value < 0)
                {
                    return Error.BadRequest("Đơn giá sản phẩm không được nhỏ hơn 0.", "Items");
                }
            }

            po.PurchaseRequestId = request.PurchaseRequestId;
            po.SupplierId = request.SupplierId;
            po.Note = request.Note;

            var insideItems = request.Items
                .Where(x => !request.PurchaseRequestId.HasValue || x.QuotationProductRowId.HasValue)
                .ToList();
            var outsideItems = request.Items
                .Where(x => request.PurchaseRequestId.HasValue && !x.QuotationProductRowId.HasValue)
                .ToList();

            var existingItemsDict = po.PurchaseOrderItems.ToDictionary(x => x.Id);
            var insideItemsDict = insideItems.Where(x => x.Id.HasValue && x.Id > 0).ToDictionary(x => x.Id!.Value);

            var toDelete = po.PurchaseOrderItems.Where(x => !insideItemsDict.ContainsKey(x.Id)).ToList();
            foreach (var item in toDelete)
            {
                deleteRepository.DeleteItem(item);
                po.PurchaseOrderItems.Remove(item);
            }

            foreach (var itemRequest in insideItems)
            {
                if (itemRequest.Id.HasValue && itemRequest.Id > 0)
                {
                    if (existingItemsDict.TryGetValue(itemRequest.Id.Value, out var existingItem))
                    {
                        existingItem.ProductVariantId = itemRequest.ProductVariantId!.Value;
                        existingItem.ProductVariantColorId = itemRequest.ProductVariantColorId;
                        existingItem.OrderedQuantity = itemRequest.OrderedQuantity!.Value;
                        existingItem.UnitPrice = itemRequest.UnitPrice!.Value;
                        existingItem.QuotationProductRowId = itemRequest.QuotationProductRowId;
                    }
                }
                else
                {
                    var newItem = new PurchaseOrderItem
                    {
                        ProductVariantId = itemRequest.ProductVariantId!.Value,
                        ProductVariantColorId = itemRequest.ProductVariantColorId,
                        OrderedQuantity = itemRequest.OrderedQuantity!.Value,
                        UnitPrice = itemRequest.UnitPrice!.Value,
                        QuotationProductRowId = itemRequest.QuotationProductRowId
                    };
                    po.PurchaseOrderItems.Add(newItem);
                }
            }

            updateRepository.Update(po);

            if (outsideItems.Count > 0)
            {
                var outsidePO = new PurchaseOrderEntity
                {
                    PurchaseRequestId = null,
                    SupplierId = request.SupplierId,
                    Status = PurchaseOrderStatus.Draft,
                    Note = request.Note,
                    CreatedBy = userId,
                    OrderDate = DateTimeOffset.UtcNow,
                    PurchaseOrderItems = [.. outsideItems.Select(item => new PurchaseOrderItem
                    {
                        ProductVariantId = item.ProductVariantId!.Value,
                        ProductVariantColorId = item.ProductVariantColorId,
                        OrderedQuantity = item.OrderedQuantity!.Value,
                        UnitPrice = item.UnitPrice!.Value,
                        QuotationProductRowId = null
                    })]
                };
                insertRepository.Add(outsidePO);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(po.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<PurchaseOrderDetailResponse?>();
        }
    }
}
