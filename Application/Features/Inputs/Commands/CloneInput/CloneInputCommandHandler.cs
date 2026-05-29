using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Constants.InventoryReceipt;
using Domain.Constants.Product;
using Mapster;
using MediatR;
using InputEntity = Domain.Entities.InventoryReceipt;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Application.Features.InventoryReceipts.Commands.CloneInput;

public sealed class CloneInputCommandHandler(
    IInputReadRepository inputReadRepository,
    IInputInsertRepository inputInsertRepository,
    IProductVariantReadRepository variantReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CloneInputCommand, Result<InputDetailResponse?>>
{
    public async Task<Result<InputDetailResponse?>> Handle(
        CloneInputCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Id.HasValue)
        {
            return Error.BadRequest("Id không được để trống", "Id");
        }
        var originalInput = await inputReadRepository.GetByIdWithDetailsAsync(
            command.Id.Value,
            cancellationToken,
            DataFetchMode.All)
            .ConfigureAwait(false);
        if (originalInput is null)
        {
            return Error.NotFound($"Phiếu nhập với Id = {command.Id.Value} không tồn tại", "Id");
        }
        var productVariantIds = originalInput.InputInfos
            .Select(p => p.QuotationProductRow != null ? p.QuotationProductRow.ProductVariantId : (p.PurchaseRequestItem != null ? p.PurchaseRequestItem.ProductVariantId : (int?)null))
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
        var validProducts = new List<InputInfoEntity>();
        foreach (var originalProduct in originalInput.InputInfos)
        {
            var resolvedVariantId = originalProduct.QuotationProductRow != null ? originalProduct.QuotationProductRow.ProductVariantId : (originalProduct.PurchaseRequestItem != null ? originalProduct.PurchaseRequestItem.ProductVariantId : (int?)null);
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
                new InputInfoEntity
                {
                    PurchaseRequestItemId = originalProduct.PurchaseRequestItemId,
                    QuotationProductRowId = originalProduct.QuotationProductRowId,
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
        var newInput = new InputEntity
        {
            Notes = originalInput.Notes,
            PurchaseRequestId = originalInput.PurchaseRequestId,
            StatusId = InputStatus.Working,
            InputInfos = validProducts,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        inputInsertRepository.Add(newInput);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var createdInput = await inputReadRepository.GetByIdWithDetailsAsync(newInput.Id, cancellationToken)
            .ConfigureAwait(false);
        return createdInput!.Adapt<InputDetailResponse>();
    }
}