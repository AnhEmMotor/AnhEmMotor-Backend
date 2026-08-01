using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Services.Shipping;
using Application.Interfaces.Services.Shipping.Models;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Domain.Enums;
using Mapster;
using MediatR;
using System.Linq;

using Application.Interfaces.Repositories.Voucher;

namespace Application.Features.Outputs.Commands.CreateOutput;

public class CreateOutputCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IProductVariantReadRepository variantRepository,
    ISettingRepository settingRepository,
    IShippingService shippingService,
    IVoucherReadRepository voucherReadRepository,
    IVoucherUsageRepository voucherUsageRepository,
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
            var nameParts = new[] { variant.Product?.Name, variant.VariantName, color?.ColorName ?? color?.ColorCode }.Where(
                part => !string.IsNullOrWhiteSpace(part));
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
        if (output.ProvinceId.HasValue)
        {
            output.ProvinceName = await shippingService.GetProvinceNameAsync(output.ProvinceId.Value, cancellationToken);
            if (!string.IsNullOrEmpty(output.WardCode))
            {
                output.WardName = await shippingService.GetWardNameAsync(
                    output.ProvinceId.Value,
                    output.WardCode,
                    cancellationToken);
            }
        }
        foreach (var info in output.OutputInfos)
        {
            var matchingVariant = variantsList.FirstOrDefault(v => v.Id == info.ProductVariantId);
            if (matchingVariant != null)
            {
                info.Price = matchingVariant.Price;
            }
        }
        if (output.ProvinceId.HasValue &&
            !string.IsNullOrEmpty(output.WardCode) &&
            !string.IsNullOrEmpty(output.CustomerAddress))
        {
            var feeRequest = new CalculateShippingFeeRequest
            {
                ToWardIdV2 = int.Parse(output.WardCode),
                ToAddressV2 = output.CustomerAddress ?? string.Empty,
                IsNewToAddress = true,
                ToWardCode = output.WardCode,
                Items =
                    output.OutputInfos
                        .Select(
                            oi =>
                            {
                                var v = variantsList.FirstOrDefault(x => x.Id == oi.ProductVariantId);
                                var p = v?.Product;
                                return new ShippingItemDto
                        {
                            Name = p?.Name ?? "Product",
                            Quantity = oi.Count ?? 1,
                            Length = (int?)(v?.Length ?? p?.Length),
                            Width = (int?)(v?.Width ?? p?.Width),
                            Height = (int?)(v?.Height ?? p?.Height),
                            Weight = (int?)((v?.Weight ?? p?.Weight) * 1000)
                        };
                            })
                        .ToList()
            };
            var feeResult = await shippingService.CalculateShippingFeeAsync(feeRequest, cancellationToken);
            if (feeResult.IsSuccess)
                output.ShippingFee = feeResult.Value;
        }
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var totalPrice = output.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0));
        bool hasVehicle = false;
        bool hasPart = false;
        bool hasAccessory = false;

        foreach (var info in output.OutputInfos)
        {
            var variant = variantsList.FirstOrDefault(v => v.Id == info.ProductVariantId);
            var managementType = variant?.Product?.ProductCategory?.ManagementType;
            var categoryName = variant?.Product?.ProductCategory?.Name ?? "";
            
            if (string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
            {
                hasVehicle = true;
            }
            else if (categoryName.Contains("Phụ kiện", StringComparison.OrdinalIgnoreCase))
            {
                hasAccessory = true;
            }
            else
            {
                hasPart = true;
            }
        }

        string orderType = "Xe máy";
        if (hasVehicle && (hasPart || hasAccessory))
        {
            orderType = "Phụ tùng & xe máy";
        }
        else if (hasVehicle)
        {
            orderType = "Xe máy";
        }
        else if (hasPart)
        {
            orderType = "Chỉ có phụ tùng";
        }
        else if (hasAccessory)
        {
            orderType = "Chỉ có phụ kiện";
        }

        var thresholdKey = $"Deposit_{orderType}_Threshold";
        var ratioKey = $"Deposit_{orderType}_Ratio";

        var thresholdSetting = settings.FirstOrDefault(s => string.Equals(s.Key, thresholdKey, StringComparison.OrdinalIgnoreCase));
        decimal threshold = 100000000;
        if (thresholdSetting != null && decimal.TryParse(thresholdSetting.Value, out var parsedThreshold))
        {
            threshold = parsedThreshold;
        }

        var ratioSetting = settings.FirstOrDefault(s => string.Equals(s.Key, ratioKey, StringComparison.OrdinalIgnoreCase));
        int ratio = 20;
        if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
        {
            ratio = parsedRatio;
        }

        if (string.IsNullOrWhiteSpace(output.StatusId))
        {
            output.StatusId = totalPrice > threshold ? OrderStatus.WaitingDeposit : OrderStatus.Pending;
        }
if (totalPrice >= threshold)
        {
            output.DepositRatio = ratio;
        }
        else
        {
            output.DepositRatio = 0;
        }
        output.BuyerId = request.BuyerId;
        output.CreatedBy = request.BuyerId;
        output.PaymentMethod = request.PaymentMethod ?? PaymentMethod.COD;
        output.PaymentStatus = "Pending";
        
        insertRepository.Add(output);

        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            var voucher = await voucherReadRepository.GetByCodeAsync(request.VoucherCode, cancellationToken).ConfigureAwait(false);
            if (voucher != null)
            {
                var today = DateTime.UtcNow.Date;
                var totalUsed = await voucherUsageRepository.GetTotalUsageCountAsync(voucher.Id, cancellationToken);
                var userUsedCount = request.BuyerId.HasValue ? await voucherUsageRepository.GetUserUsageCountAsync(voucher.Id, request.BuyerId.Value, cancellationToken) : 0;
                
                var isValid = today >= voucher.ValidFrom.Date && today <= voucher.ValidTo.Date
                    && (voucher.TotalUsageLimit == 0 || totalUsed < voucher.TotalUsageLimit)
                    && (voucher.UsageLimitPerUser == 0 || userUsedCount < voucher.UsageLimitPerUser)
                    && (voucher.MinOrderValue == 0 || totalPrice >= voucher.MinOrderValue);

                if (isValid)
                {
                    var discountAmount = voucher.DiscountType == DiscountType.Percent
                        ? voucher.DiscountValue * totalPrice / 100
                        : voucher.DiscountValue;
                        
                    if (voucher.MaxDiscountAmount > 0 && discountAmount > voucher.MaxDiscountAmount)
                        discountAmount = voucher.MaxDiscountAmount.Value;

                    discountAmount = Math.Min(discountAmount, totalPrice);

                    var orderVoucher = new OrderVoucher
                    {
                        VoucherId = voucher.Id,
                        OutputId = output.Id, // EF Core will fix up the ID upon SaveChangesAsync, actually wait!
                        Output = output,
                        DiscountApplied = discountAmount,
                        AppliedAt = DateTimeOffset.UtcNow,
                        AppliedBy = request.BuyerId?.ToString() ?? "System"
                    };
                    await voucherUsageRepository.AddAsync(orderVoucher, cancellationToken);
                }
            }
        }

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
        return productVariantColorId.HasValue && productVariantColorId.Value > 0 ? productVariantColorId.Value : null;
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

