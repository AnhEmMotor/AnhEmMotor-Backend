using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Setting;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public class CreateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IProductVariantReadRepository variantRepository,
    ISettingRepository settingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOutputCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        CreateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var variantIds = request.OutputInfos
            .Where(p => p.ProductVariantId.HasValue)
            .Select(p => p.ProductVariantId!.Value)
            .Distinct()
            .ToList();
        var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        var variantsList = variants.ToList();
        if (variantsList.Count != variantIds.Count)
        {
            var foundIds = variantsList.Select(v => v.Id).ToList();
            var missingIds = variantIds.Except(foundIds).ToList();
            return Error.NotFound(
                $"Không tìm thấy {missingIds.Count} sản phẩm: {string.Join(", ", missingIds)}",
                "Products");
        }
        foreach (var variant in variantsList)
        {
            if (string.Compare(variant.Product?.StatusId, Domain.Constants.Product.ProductStatus.ForSale) != 0)
            {
                return Error.BadRequest(
                    $"Sản phẩm '{variant.Product?.Name ?? variant.Id.ToString()}' không còn được bán.",
                    "Products");
            }
        }
        var errors = new List<Error>();
        for (int i = 0; i < request.OutputInfos.Count; i++)
        {
            var info = request.OutputInfos[i];
            if (!info.ProductVariantId.HasValue)
            {
                errors.Add(Error.BadRequest("ProductVariantId là bắt buộc.", $"products[{i}].productVariantId"));
                continue;
            }
            var variant = variantsList.FirstOrDefault(v => v.Id == info.ProductVariantId.Value);
            var colorValidation = ValidateVariantColor(variant, info.ProductVariantColorId);
            if (colorValidation is not null)
            {
                errors.Add(colorValidation);
            }
        }
        foreach (var group in request.OutputInfos
            .Select((Info, Index) => new { Info, Index })
            .Where(x => x.Info.ProductVariantId.HasValue)
            .GroupBy(
                x => new
                {
                    ProductVariantId = x.Info.ProductVariantId!.Value,
                    ProductVariantColorId = NormalizeColorId(x.Info.ProductVariantColorId)
                }))
        {
            var variant = variantsList.FirstOrDefault(v => v.Id == group.Key.ProductVariantId);
            if (variant is null)
            {
                continue;
            }
            var color = group.Key.ProductVariantColorId.HasValue
                ? variant.ProductVariantColors.FirstOrDefault(c => c.Id == group.Key.ProductVariantColorId.Value)
                : null;
            if (group.Key.ProductVariantColorId.HasValue && color is null)
            {
                continue;
            }
            var effectiveMax = GetEffectiveMaxPurchaseQuantity(variant, color);
            if (!effectiveMax.HasValue)
            {
                continue;
            }
            var totalCount = group.Sum(x => x.Info.Count ?? 0);
            if (totalCount <= effectiveMax.Value)
            {
                continue;
            }
            var nameParts = new[]
            {
                variant.Product?.Name,
                variant.VariantName,
                color?.ColorName ?? color?.ColorCode
            }.Where(part => !string.IsNullOrWhiteSpace(part));
            errors.Add(
                Error.BadRequest(
                    $"Số lượng mua tối đa cho sản phẩm '{string.Join(" - ", nameParts)}' là {effectiveMax.Value} sản phẩm.",
                    $"products[{group.Min(x => x.Index)}]"));
        }
        if (errors.Count > 0)
        {
            return Result<OrderDetailResponse>.Failure(errors);
        }
        var output = request.Adapt<Output>();
        foreach (var info in output.OutputInfos)
        {
            var matchingVariant = variantsList.FirstOrDefault(v => v.Id == info.ProductVariantId);
            if (matchingVariant != null)
            {
                info.Price = matchingVariant.Price;
            }
        }
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(output.StatusId))
        {
            var totalPrice = output.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0));
            var thresholdSetting = settings.FirstOrDefault(
                s => string.Equals(s.Key, SettingKeys.OrderValueExceeds, StringComparison.OrdinalIgnoreCase));
            decimal threshold = 100000000;
            if (thresholdSetting != null && decimal.TryParse(thresholdSetting.Value, out var parsedThreshold))
            {
                threshold = parsedThreshold;
            }
            output.StatusId = totalPrice >= threshold ? OrderStatus.WaitingDeposit : OrderStatus.Pending;
        }
        var ratioSetting = settings.FirstOrDefault(
            s => string.Equals(s.Key, SettingKeys.DepositRatio, StringComparison.OrdinalIgnoreCase));
        if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
        {
            output.DepositRatio = parsedRatio;
        }
        else
        {
            output.DepositRatio = 50;
        }
        output.BuyerId = request.BuyerId;
        output.CreatedBy = request.BuyerId;
        output.PaymentMethod = request.PaymentMethod ?? PaymentMethod.COD;
        output.PaymentStatus = "Pending";
        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var created = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(created);
        return created.Adapt<OrderDetailResponse>();
    }

    private static Error? ValidateVariantColor(ProductVariant? variant, int? productVariantColorId)
    {
        if (variant is null)
        {
            return null;
        }
        if (variant.ProductVariantColors.Count == 0)
        {
            return productVariantColorId.HasValue && productVariantColorId.Value > 0
                ? Error.BadRequest("Biến thể sản phẩm này không có màu sắc để chọn.", "ProductVariantColorId")
                : null;
        }
        if (!productVariantColorId.HasValue || productVariantColorId <= 0)
        {
            return Error.BadRequest(
                "Biến thể sản phẩm có màu sắc, ProductVariantColorId là bắt buộc.",
                "ProductVariantColorId");
        }
        return variant.ProductVariantColors.Any(c => c.Id == productVariantColorId.Value)
            ? null
            : Error.BadRequest("ProductVariantColorId không thuộc biến thể sản phẩm đã chọn.", "ProductVariantColorId");
    }

    private static int? NormalizeColorId(int? productVariantColorId)
    {
        return productVariantColorId.HasValue && productVariantColorId.Value > 0
            ? productVariantColorId.Value
            : null;
    }

    private static int? GetEffectiveMaxPurchaseQuantity(ProductVariant variant, ProductVariantColor? color)
    {
        if (color?.MaxPurchaseQuantity.HasValue == true)
        {
            return color.MaxPurchaseQuantity.Value;
        }
        if (variant.MaxPurchaseQuantity.HasValue)
        {
            return variant.MaxPurchaseQuantity.Value;
        }
        return GetEffectiveMaxPurchaseQuantity(variant.Product?.ProductCategory);
    }

    private static int? GetEffectiveMaxPurchaseQuantity(ProductCategory? category)
    {
        var current = category;
        while (current != null)
        {
            if (current.MaxPurchaseQuantity.HasValue)
            {
                return current.MaxPurchaseQuantity.Value;
            }
            current = current.Parent;
        }
        return null;
    }
}
