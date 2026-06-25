using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
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
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest
{
    public class CreatePurchaseRequestCommandHandler(
        IPurchaseRequestInsertRepository insertRepository,
        IPurchaseRequestReadRepository readRepository,
        IProductVariantReadRepository variantRepository,
        ISupplierReadRepository supplierReadRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePurchaseRequestCommand, Result<PurchaseRequestDetailResponse?>>
    {
        public async Task<Result<PurchaseRequestDetailResponse?>> Handle(
            CreatePurchaseRequestCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Items.Count == 0)
            {
                return Error.BadRequest("Danh sách sản phẩm yêu cầu không được trống.", "Items");
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
                .Distinct()
                .ToList();
            var suppliers = await supplierReadRepository.GetByIdAsync(supplierIds, cancellationToken, DataFetchMode.ActiveOnly).ConfigureAwait(false);
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
            var purchaseRequest = new PurchaseRequestEntity
            {
                Status = "draft",
                Note = request.Note,
                CreatedBy = currentUserId,
                PurchaseRequestItems =
                    [.. request.Items
                        .Select(
                            item => new PurchaseRequestItem
                        {
                            ProductVariantId = item.ProductVariantId!.Value,
                            ProductVariantColorId = item.ProductVariantColorId,
                            Quantity = item.Quantity!.Value,
                            SupplierId = item.SupplierId,
                            ProductQuotationId = item.ProductQuotationId,
                            UnitPrice = item.UnitPrice
                        })]
            };
            insertRepository.Add(purchaseRequest);

            var auditLogs = new List<Domain.Entities.PurchaseRequestAuditLog>
            {
                new Domain.Entities.PurchaseRequestAuditLog
                {
                    PurchaseRequest = purchaseRequest,
                    Action = "Add",
                    ChangedById = currentUserId,
                    ChangedAt = DateTimeOffset.UtcNow,
                    NewStatusId = purchaseRequest.Status,
                    NewNotes = purchaseRequest.Note
                }
            };
            await insertRepository.InsertAuditLogsAsync(auditLogs, cancellationToken).ConfigureAwait(false);

            var itemAuditLogs = purchaseRequest.PurchaseRequestItems.Select(item => new Domain.Entities.PurchaseRequestItemAuditLog
            {
                PurchaseRequestItem = item,
                Action = "Add",
                NewQuantity = item.Quantity,
                NewProductVariantId = item.ProductVariantId,
                NewProductVariantColorId = item.ProductVariantColorId,
                NewSupplierName = item.SupplierId.HasValue && supplierDict.TryGetValue(item.SupplierId.Value, out var supplierName) ? supplierName : null,
                NewUnitPrice = item.UnitPrice
            }).ToList();
            await insertRepository.InsertItemAuditLogsAsync(itemAuditLogs, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var created = await readRepository.GetByIdWithDetailsAsync(purchaseRequest.Id, cancellationToken)
                .ConfigureAwait(false);
            return created!.Adapt<PurchaseRequestDetailResponse?>();
        }
    }
}
