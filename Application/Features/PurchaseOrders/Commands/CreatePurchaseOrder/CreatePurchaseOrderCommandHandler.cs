using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseOrder;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PurchaseOrderEntity = Domain.Entities.PurchaseOrder;

namespace Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder
{
    public sealed class CreatePurchaseOrderCommandHandler(
        IPurchaseOrderInsertRepository insertRepository,
        IPurchaseOrderReadRepository readRepository,
        IProductVariantReadRepository variantRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderDetailResponse?>>
    {
        public async Task<Result<PurchaseOrderDetailResponse?>> Handle(
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

            var currentUserId = currentUserContext.GetUserId();
            var purchaseOrder = new PurchaseOrderEntity
            {
                PurchaseRequestId = request.PurchaseRequestId,
                SupplierId = request.SupplierId,
                Status = PurchaseOrderStatus.Draft,
                Note = request.Note,
                CreatedBy = currentUserId,
                OrderDate = DateTimeOffset.UtcNow,
                PurchaseOrderItems =
                    [.. request.Items
                        .Select(
                            item => new PurchaseOrderItem
                            {
                                ProductVariantId = item.ProductVariantId!.Value,
                                ProductVariantColorId = item.ProductVariantColorId,
                                OrderedQuantity = item.OrderedQuantity!.Value,
                                UnitPrice = item.UnitPrice!.Value
                            })]
            };

            insertRepository.Add(purchaseOrder);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var created = await readRepository.GetByIdWithDetailsAsync(purchaseOrder.Id, cancellationToken)
                .ConfigureAwait(false);

            return created!.Adapt<PurchaseOrderDetailResponse?>();
        }
    }
}
