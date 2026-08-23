using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Output;
using Application.Interfaces.Repositories.Logistics.Shipment;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Setting;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services.Shipping;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Domain.Entities.Logistics;
using Domain.Constants.Logistics;
using Mapster;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutputByManager;

public class CreateOutputByManagerCommandHandler(
    IOutputReadRepository readRepository,
    IOutputInsertRepository insertRepository,
    IOutputUpdateRepository updateRepository,
    IProductVariantReadRepository variantRepository,
    IUserReadRepository userReadRepository,
    ISettingRepository settingRepository,
    IShippingService shippingService,
    IUnitOfWork unitOfWork,
    IShipmentInsertRepository? shipmentInsertRepository = null) : IRequestHandler<CreateOutputByManagerCommand, Result<OrderDetailResponse>>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        CreateOutputByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var userData = await userReadRepository.GetUserByIDAsync(request.BuyerId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if (userData == null)
        {
            return Error.Forbidden(
                "ID này là 1 tài khoản không tồn tại/đã bị xoá/đã bị cấm. Vui lòng kiểm tra lại.",
                "BuyerId");
        }
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
        foreach (var product in request.OutputInfos.Where(p => p.ProductVariantId.HasValue))
        {
            var variant = variantsList.First(v => v.Id == product.ProductVariantId!.Value);
            var colorValidation = ValidateVariantColor(variant, product.ProductVariantColorId);
            if (colorValidation is not null)
            {
                return colorValidation;
            }
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
        var settings = await settingRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var totalPrice = output.OutputInfos.Sum(i => (i.Price ?? 0) * (i.Count ?? 0));
        bool hasVehicle = false;
        bool hasPart = false;
        bool hasAccessory = false;
        foreach (var info in output.OutputInfos)
        {
            var variant = variantsList.FirstOrDefault(v => v.Id == info.ProductVariantId);
            var managementType = variant?.Product?.ProductCategory?.ManagementType;
            var categoryName = variant?.Product?.ProductCategory?.Name ?? string.Empty;
            if (string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
            {
                hasVehicle = true;
            } else if (categoryName.Contains("Phụ kiện", StringComparison.OrdinalIgnoreCase))
            {
                hasAccessory = true;
            } else
            {
                hasPart = true;
            }
        }
        string orderType = "Xe máy";
        if (hasVehicle && (hasPart || hasAccessory))
        {
            orderType = "Phụ tùng & xe máy";
        } else if (hasVehicle)
        {
            orderType = "Xe máy";
        } else if (hasPart)
        {
            orderType = "Chỉ có phụ tùng";
        } else if (hasAccessory)
        {
            orderType = "Chỉ có phụ kiện";
        }
        var thresholdKey = $"Deposit_{orderType}_Threshold";
        var ratioKey = $"Deposit_{orderType}_Ratio";
        var thresholdSetting = settings.FirstOrDefault(
            s => string.Equals(s.Key, thresholdKey, StringComparison.OrdinalIgnoreCase));
        decimal threshold = 100000000;
        if (thresholdSetting != null && decimal.TryParse(thresholdSetting.Value, out var parsedThreshold))
        {
            threshold = parsedThreshold;
        }
        var ratioSetting = settings.FirstOrDefault(
            s => string.Equals(s.Key, ratioKey, StringComparison.OrdinalIgnoreCase));
        int ratio = 20;
        if (ratioSetting != null && int.TryParse(ratioSetting.Value, out var parsedRatio))
        {
            ratio = parsedRatio;
        }
        if (request.DepositRatio.HasValue)
        {
            output.DepositRatio = request.DepositRatio.Value;
        } else if (totalPrice >= threshold)
        {
            output.DepositRatio = ratio;
        } else
        {
            output.DepositRatio = 0;
        }
        if (string.IsNullOrWhiteSpace(output.StatusId))
        {
            output.StatusId = totalPrice > threshold ? OrderStatus.WaitingDeposit : OrderStatus.Pending;
        }
        insertRepository.Add(output);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (string.Compare(output.StatusId, OrderStatus.Completed) == 0)
        {
            output.FinishedBy = request.CurrentUserId;
            updateRepository.Update(output);
            var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, true, cancellationToken)
                .ConfigureAwait(false);
            if (result.IsFailure)
            {
                return Result<OrderDetailResponse>.Failure(result.Errors!);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        } else if (string.Compare(output.StatusId, OrderStatus.Delivering) == 0)
        {
            var result = await updateRepository.HandleInventoryTransactionAsync(output.Id, false, cancellationToken)
                .ConfigureAwait(false);
            if (result.IsFailure)
            {
                return Result<OrderDetailResponse>.Failure(result.Errors!);
            }
            if (shipmentInsertRepository != null)
            {
                await shipmentInsertRepository.AddAsync(
                    new Shipment
                    {
                        CustomerName = output.CustomerName ?? string.Empty,
                        CustomerPhone = output.CustomerPhone ?? string.Empty,
                        DestinationAddress = output.CustomerAddress ?? string.Empty,
                        ShippingCost = output.ShippingFee ?? 0,
                        CodAmount = output.Total - (output.PaidAmount ?? 0),
                        OriginAddress = "Kho AnhEmMotor",
                        OutputId = output.Id,
                        Type = ShipmentType.OrderDelivery,
                        Items = output.OutputInfos.Select(
                                info => new ShipmentItem
                                {
                                    ProductVariantId = info.ProductVariantId,
                                    ProductVariantColorId = info.ProductVariantColorId,
                                    Quantity = info.Count ?? 1
                                })
                            .ToList()
                    },
                    cancellationToken).ConfigureAwait(false);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        var created = await readRepository.GetByIdWithDetailsAsync(output.Id, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(created);
        return created.Adapt<OrderDetailResponse>();
    }

    private static Error? ValidateVariantColor(ProductVariant variant, int? productVariantColorId)
    {
        if (variant.ProductVariantColors.Count == 0)
        {
            return productVariantColorId.HasValue
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
}

