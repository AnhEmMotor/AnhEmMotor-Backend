using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseOrder;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;
using Application.ApiContracts.PurchaseOrder.Requests;

namespace Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder
{
    public sealed class CreatePurchaseOrderCommandHandler(
        IPurchaseOrderInsertRepository insertRepository,
        IPurchaseOrderReadRepository readRepository,
        IProductVariantReadRepository variantRepository,
        IPurchaseRequestReadRepository prReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePurchaseOrderCommand, Result<List<PurchaseOrderDetailResponse>>>
    {
        public async Task<Result<List<PurchaseOrderDetailResponse>>> Handle(
            CreatePurchaseOrderCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Items.Count == 0)
            {
                return Error.BadRequest("Danh sách sản phẩm chốt mua không được trống.", "Items");
            }

            var variantIds = request.Items
                .Where(x => x.ProductVariantId.HasValue)
                .Select(x => x.ProductVariantId!.Value)
                .Distinct()
                .ToList();

            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            var variantDict = variants.ToDictionary(v => v.Id);

            // Get PR details if it exists to check remaining quantities
            PurchaseRequest? pr = null;
            if (request.PurchaseRequestId.HasValue)
            {
                pr = await prReadRepository.GetByIdWithDetailsAsync(request.PurchaseRequestId.Value, cancellationToken)
                    .ConfigureAwait(false);
            }

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
                 if (!item.SupplierId.HasValue)
                {
                    return Error.BadRequest("SupplierId của từng sản phẩm không được trống.", "Items");
                }
            }

            var currentUserId = currentUserContext.GetUserId();
            
            // Logic to split items based on PR remaining quantity
            var finalItems = new List<CreatePurchaseOrderItemRequest>();
            if (pr != null)
            {
                foreach (var item in request.Items)
                {
                    if (item.PurchaseRequestItemId.HasValue)
                    {
                        var prItem = pr.PurchaseRequestItems.FirstOrDefault(x => x.Id == item.PurchaseRequestItemId.Value);
                        if (prItem != null)
                        {
                            var poQty = prItem.PurchaseOrderItems
                                .Where(poi => poi.PurchaseOrder != null && 
                                              poi.PurchaseOrder.DeletedAt == null &&
                                              (string.Compare(poi.PurchaseOrder.Status, PurchaseOrderStatus.Draft, StringComparison.OrdinalIgnoreCase) == 0 ||
                                               string.Compare(poi.PurchaseOrder.Status, PurchaseOrderStatus.Sent, StringComparison.OrdinalIgnoreCase) == 0 ||
                                               string.Compare(poi.PurchaseOrder.Status, PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase) == 0))
                                .Sum(poi => poi.OrderedQuantity);
                            
                            var remaining = prItem.Quantity - poQty;
                            if (remaining <= 0)
                            {
                                // No remaining PR quantity, treat as outside PR
                                finalItems.Add(new CreatePurchaseOrderItemRequest
                                {
                                    ProductVariantId = item.ProductVariantId,
                                    ProductVariantColorId = item.ProductVariantColorId,
                                    OrderedQuantity = item.OrderedQuantity,
                                    UnitPrice = item.UnitPrice,
                                    SupplierId = item.SupplierId,
                                    PurchaseRequestItemId = null,
                                    QuotationProductRowId = null
                                });
                            }
                            else if (item.OrderedQuantity > remaining)
                            {
                                // Exceeds PR, split into two
                                finalItems.Add(new CreatePurchaseOrderItemRequest
                                {
                                    ProductVariantId = item.ProductVariantId,
                                    ProductVariantColorId = item.ProductVariantColorId,
                                    OrderedQuantity = remaining,
                                    UnitPrice = item.UnitPrice,
                                    SupplierId = item.SupplierId,
                                    PurchaseRequestItemId = item.PurchaseRequestItemId,
                                    QuotationProductRowId = item.QuotationProductRowId
                                });
                                finalItems.Add(new CreatePurchaseOrderItemRequest
                                {
                                    ProductVariantId = item.ProductVariantId,
                                    ProductVariantColorId = item.ProductVariantColorId,
                                    OrderedQuantity = item.OrderedQuantity - remaining,
                                    UnitPrice = item.UnitPrice,
                                    SupplierId = item.SupplierId,
                                    PurchaseRequestItemId = null,
                                    QuotationProductRowId = null
                                });
                            }
                            else
                            {
                                // Fits in PR
                                finalItems.Add(item);
                            }
                        }
                        else
                        {
                            finalItems.Add(item);
                        }
                    }
                    else
                    {
                        finalItems.Add(item);
                    }
                }
            }
            else
            {
                finalItems.AddRange(request.Items);
            }

            var groupedItems = finalItems.GroupBy(x => new
            {
                SupplierId = x.SupplierId!.Value,
                IsOutsidePR = !request.PurchaseRequestId.HasValue || !x.PurchaseRequestItemId.HasValue
            }).ToList();
            var createdPOs = new List<PurchaseOrderEntity>();

            foreach (var group in groupedItems)
            {
                var supplierId = group.Key.SupplierId;
                var isOutside = group.Key.IsOutsidePR;
                var purchaseOrder = new PurchaseOrderEntity
                {
                    PurchaseRequestId = isOutside ? null : request.PurchaseRequestId,
                    SupplierId = supplierId,
                    Status = PurchaseOrderStatus.Draft,
                    Note = request.Note,
                    CreatedBy = currentUserId,
                    OrderDate = DateTimeOffset.UtcNow,
                    PurchaseOrderItems = [.. group.Select(item => new PurchaseOrderItem
                    {
                        ProductVariantId = item.ProductVariantId!.Value,
                        ProductVariantColorId = item.ProductVariantColorId,
                        OrderedQuantity = item.OrderedQuantity!.Value,
                        UnitPrice = item.UnitPrice!.Value,
                        QuotationProductRowId = item.QuotationProductRowId,
                        PurchaseRequestItemId = item.PurchaseRequestItemId
                    })]
                };
                insertRepository.Add(purchaseOrder);
                createdPOs.Add(purchaseOrder);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var responseList = new List<PurchaseOrderDetailResponse>();
            foreach (var po in createdPOs)
            {
                var created = await readRepository.GetByIdWithDetailsAsync(po.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (created != null)
                {
                    responseList.Add(created.Adapt<PurchaseOrderDetailResponse>());
                }
            }

            return responseList;
        }
    }
}
