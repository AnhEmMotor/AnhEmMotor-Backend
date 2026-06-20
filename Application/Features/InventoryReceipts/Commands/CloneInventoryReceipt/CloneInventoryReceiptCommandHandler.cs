using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;
using Domain.Constants.InventoryReceipt;
using Domain.Constants.Product;
using Mapster;
using MediatR;
using System;
using System.Linq;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

namespace Application.Features.InventoryReceipts.Commands.CloneInventoryReceipt
{
    public class CloneInventoryReceiptCommandHandler(
        IInventoryReceiptReadRepository InventoryReceiptReadRepository,
        IInventoryReceiptInsertRepository InventoryReceiptInsertRepository,
        IProductVariantReadRepository variantReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CloneInventoryReceiptCommand, Result<InventoryReceiptDetailResponse?>>
    {
        public async Task<Result<InventoryReceiptDetailResponse?>> Handle(
            CloneInventoryReceiptCommand command,
            CancellationToken cancellationToken)
        {
            if (!command.Id.HasValue)
            {
                return Error.BadRequest("Id không được để trống", "Id");
            }
            var originalInventoryReceipt = await InventoryReceiptReadRepository.GetByIdWithDetailsAsync(
                command.Id.Value,
                cancellationToken,
                DataFetchMode.All)
                .ConfigureAwait(false);
            if (originalInventoryReceipt is null)
            {
                return Error.NotFound($"Phiếu nhập với Id = {command.Id.Value} không tồn tại", "Id");
            }
            var productVariantIds = originalInventoryReceipt.InventoryReceiptInfos
                .Select(p => p.PurchaseRequestItem != null ? p.PurchaseRequestItem.ProductVariantId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            var variants = await variantReadRepository.GetByIdAsync(
                productVariantIds,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            var variantDict = variants.ToDictionary(v => v.Id);
            var validProducts = new List<InventoryReceiptInfoEntity>();
            foreach (var originalProduct in originalInventoryReceipt.InventoryReceiptInfos)
            {
                var resolvedVariantId = originalProduct.PurchaseRequestItem != null
                    ? originalProduct.PurchaseRequestItem.ProductVariantId
                    : (int?)null;
                if (!resolvedVariantId.HasValue)
                {
                    continue;
                }
                if (!variantDict.TryGetValue(resolvedVariantId.Value, out var variant))
                {
                    continue;
                }
                if (string.Compare(variant.Product?.StatusId, ProductStatus.ForSale) != 0)
                {
                    continue;
                }
                validProducts.Add(
                    new InventoryReceiptInfoEntity
                    {
                        PurchaseRequestItemId = originalProduct.PurchaseRequestItemId,
                        Count = originalProduct.Count,
                        RemainingCount = originalProduct.Count,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
            }
            if (validProducts.Count == 0)
            {
                return Error.BadRequest(
                    "Tất cả sản phẩm trong phiếu nhập gốc đều không còn hợp lệ (đã xóa hoặc không còn bán)",
                    "Products");
            }
            var newInventoryReceipt = new InventoryReceiptEntity
            {
                Notes = originalInventoryReceipt.Notes,
                PurchaseRequestId = originalInventoryReceipt.PurchaseRequestId,
                StatusId = InventoryReceiptStatus.Draft,
                InventoryReceiptInfos = validProducts,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            InventoryReceiptInsertRepository.Add(newInventoryReceipt);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var createdInventoryReceipt = await InventoryReceiptReadRepository.GetByIdWithDetailsAsync(
                newInventoryReceipt.Id,
                cancellationToken)
                .ConfigureAwait(false);
            return createdInventoryReceipt!.Adapt<InventoryReceiptDetailResponse>();
        }
    }
}