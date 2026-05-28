using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Permission.Permissions;
using Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest
{
    public sealed class UpdatePurchaseRequestCommandHandler(
        IPurchaseRequestReadRepository readRepository,
        IPurchaseRequestUpdateRepository updateRepository,
        IPurchaseRequestDeleteRepository deleteRepository,
        IProductVariantReadRepository variantRepository,
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

            // State and permission checks:
            if (string.Equals(pr.Status, "sent", StringComparison.OrdinalIgnoreCase))
            {
                var userId = currentUserContext.GetUserId();
                if (!await permissionReadRepository.CheckUserPermissionsAsync(userId, [Domain.Constants.Permission.Permissions.PurchaseRequests.ApproveReject], cancellationToken))
                {
                    return Error.BadRequest("Bạn không có quyền chỉnh sửa yêu cầu mua hàng khi đã ở trạng thái Sent.", "Status");
                }
            }
            else if (string.Equals(pr.Status, "approve", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(pr.Status, "reject", StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest("Không thể chỉnh sửa yêu cầu mua hàng đã được Phê duyệt hoặc Từ chối.", "Status");
            }

            var variantIds = request.Items
                .Where(x => x.ProductVariantId.HasValue)
                .Select(x => x.ProductVariantId!.Value)
                .Distinct()
                .ToList();

            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            var variantDict = variants.ToDictionary(v => v.Id);

            // Validate all items in the request
            foreach (var item in request.Items)
            {
                if (!item.ProductVariantId.HasValue)
                {
                    return Error.BadRequest("ProductVariantId không được để trống.", "Items");
                }

                if (!variantDict.TryGetValue(item.ProductVariantId.Value, out var variant))
                {
                    return Error.NotFound($"Không tìm thấy biến thể sản phẩm có ID {item.ProductVariantId.Value}.", "Items");
                }

                // Rule: Nếu có màu sắc bên trong biến thể sản phẩm thì bắt buộc phải chọn, không thì không chọn
                var colors = variant.ProductVariantColors ?? [];
                if (colors.Count > 0)
                {
                    if (!item.ProductVariantColorId.HasValue)
                    {
                        return Error.BadRequest($"Sản phẩm '{variant.Product?.Name ?? variant.VariantName}' yêu cầu chọn màu sắc.", "Items");
                    }
                    if (colors.All(c => c.Id != item.ProductVariantColorId.Value))
                    {
                        return Error.BadRequest($"Màu sắc đã chọn không thuộc sản phẩm '{variant.Product?.Name ?? variant.VariantName}'.", "Items");
                    }
                }
                else
                {
                    if (item.ProductVariantColorId.HasValue)
                    {
                        return Error.BadRequest($"Sản phẩm '{variant.Product?.Name ?? variant.VariantName}' không hỗ trợ chọn màu sắc.", "Items");
                    }
                }

                if (!item.Quantity.HasValue || item.Quantity.Value <= 0)
                {
                    return Error.BadRequest("Số lượng sản phẩm yêu cầu phải lớn hơn 0.", "Items");
                }
            }

            pr.Note = request.Note;

            // Sync items (Delete, Update, Add)
            var existingItemsDict = pr.PurchaseRequestItems.ToDictionary(x => x.Id);
            var requestItemsDict = request.Items.Where(x => x.Id.HasValue && x.Id > 0).ToDictionary(x => x.Id!.Value);

            // 1. Delete items not in request
            var toDelete = pr.PurchaseRequestItems.Where(x => !requestItemsDict.ContainsKey(x.Id)).ToList();
            foreach (var item in toDelete)
            {
                deleteRepository.DeleteItem(item);
                pr.PurchaseRequestItems.Remove(item);
            }

            // 2. Update or Add items
            foreach (var itemRequest in request.Items)
            {
                if (itemRequest.Id.HasValue && itemRequest.Id > 0)
                {
                    if (existingItemsDict.TryGetValue(itemRequest.Id.Value, out var existingItem))
                    {
                        existingItem.ProductVariantId = itemRequest.ProductVariantId!.Value;
                        existingItem.ProductVariantColorId = itemRequest.ProductVariantColorId;
                        existingItem.Quantity = itemRequest.Quantity!.Value;
                    }
                }
                else
                {
                    var newItem = new PurchaseRequestItem
                    {
                        ProductVariantId = itemRequest.ProductVariantId!.Value,
                        ProductVariantColorId = itemRequest.ProductVariantColorId,
                        Quantity = itemRequest.Quantity!.Value
                    };
                    pr.PurchaseRequestItems.Add(newItem);
                }
            }

            updateRepository.Update(pr);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updated = await readRepository.GetByIdWithDetailsAsync(pr.Id, cancellationToken).ConfigureAwait(false);
            return updated!.Adapt<PurchaseRequestDetailResponse?>();
        }
    }
}
