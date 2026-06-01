using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseOrder;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;
using ProductVariant = Domain.Entities.ProductVariant;
using Vehicle = Domain.Entities.Vehicle;

namespace Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;

public sealed partial class CreateInventoryReceiptCommandHandler(
    IInventoryReceiptInsertRepository insertRepository,
    IInventoryReceiptReadRepository readRepository,
    IPurchaseOrderReadRepository poReadRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IVehicleReadRepository vehicleReadRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext? currentUserContext = null) : IRequestHandler<CreateInventoryReceiptCommand, Result<InventoryReceiptDetailResponse?>>
{
    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlTagRegex();

    public async Task<Result<InventoryReceiptDetailResponse?>> Handle(
        CreateInventoryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        PurchaseOrder? po = null;
        if (request.PurchaseOrderId.HasValue)
        {
            po = await poReadRepository.GetByIdWithDetailsAsync(request.PurchaseOrderId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (po is null)
            {
                return Error.NotFound(
                    $"Đơn mua hàng {request.PurchaseOrderId} không tồn tại hoặc đã bị xóa.",
                    "PurchaseOrderId");
            }
            if (!string.Equals(po.Status, PurchaseOrderStatus.Approved, StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest(
                    $"Đơn mua hàng {request.PurchaseOrderId} chưa được phê duyệt.",
                    "PurchaseOrderId");
            }
        }

        var poItemsDict = po != null
            ? po.PurchaseOrderItems.ToDictionary(x => x.Id)
            : [];

        var variantMap = new Dictionary<int, ProductVariant>();
        var uniqueVins = new HashSet<(string Vin, int ProductVariantId, int? ProductVariantColorId)>();
        var uniqueEngines = new HashSet<(string Engine, int ProductVariantId, int? ProductVariantColorId)>();

        foreach (var product in request.Products)
        {
            var resolvedVariantId = product.PurchaseOrderItemId.HasValue &&
                    poItemsDict.TryGetValue(product.PurchaseOrderItemId.Value, out var poItem)
                ? poItem.ProductVariantId
                : (int?)null;

            var resolvedColorId = product.PurchaseOrderItemId.HasValue &&
                    poItemsDict.TryGetValue(product.PurchaseOrderItemId.Value, out var poItem2)
                ? poItem2.ProductVariantColorId
                : (int?)null;

            var resolvedSupplierId = po?.SupplierId;

            if (resolvedVariantId.HasValue)
            {
                var variants = await variantRepository.GetByIdAsync(
                    [resolvedVariantId.Value],
                    cancellationToken,
                    DataFetchMode.ActiveOnly)
                    .ConfigureAwait(false);
                var variant = variants.FirstOrDefault();
                if (variant is null)
                {
                    return Error.BadRequest(
                        $"Biến thể sản phẩm {resolvedVariantId.Value} không tồn tại hoặc đã bị xóa.",
                        "Products");
                }

                if (resolvedSupplierId.HasValue)
                {
                    var supplier = await supplierRepository.GetByIdAsync(
                        resolvedSupplierId.Value,
                        cancellationToken,
                        DataFetchMode.ActiveOnly)
                        .ConfigureAwait(false);
                    if (supplier is null || string.Compare(supplier.StatusId, Domain.Constants.SupplierStatus.Active) != 0)
                    {
                        return Error.BadRequest(
                            $"Nhà cung cấp với ID {resolvedSupplierId.Value} không tồn tại hoặc không còn hoạt động.",
                            "Products");
                    }
                }

                var colorValidation = ValidateVariantColor(variant, resolvedColorId);
                if (colorValidation is not null)
                {
                    return colorValidation;
                }

                variantMap[resolvedVariantId.Value] = variant;
                var managementType = variant.Product?.ProductCategory?.ManagementType;

                if (string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
                {
                    if (product.Vehicles == null || product.Vehicles.Count != (product.Count ?? 0))
                    {
                        return Error.BadRequest(
                            $"Danh sách xe (Vehicles) phải có đúng {product.Count ?? 0} phần tử cho sản phẩm quản lý theo số khung.",
                            "Products");
                    }

                    foreach (var v in product.Vehicles)
                    {
                        if (string.IsNullOrWhiteSpace(v.VinNumber) || string.IsNullOrWhiteSpace(v.EngineNumber))
                        {
                            return Error.BadRequest(
                                "Số khung (VinNumber) và Số máy (EngineNumber) không được để trống.",
                                "Products");
                        }

                        var vin = v.VinNumber.Trim();
                        var engine = v.EngineNumber.Trim();
                        var normalizedVinKey = (vin.ToUpperInvariant(), resolvedVariantId.Value, resolvedColorId);
                        if (!uniqueVins.Add(normalizedVinKey))
                        {
                            return Error.BadRequest($"Số khung trùng lặp trong yêu cầu: {vin}", "Products");
                        }

                        var normalizedEngineKey = (engine.ToUpperInvariant(), resolvedVariantId.Value, resolvedColorId);
                        if (!uniqueEngines.Add(normalizedEngineKey))
                        {
                            return Error.BadRequest($"Số máy trùng lặp trong yêu cầu: {engine}", "Products");
                        }

                        var isVinExists = await vehicleReadRepository.ExistsByVinAsync(
                            vin,
                            resolvedVariantId.Value,
                            resolvedColorId,
                            cancellationToken)
                            .ConfigureAwait(false);
                        if (isVinExists)
                        {
                            return Error.BadRequest($"Số khung (VIN) {vin} đã tồn tại trong hệ thống.", "Products");
                        }

                        var isEngineExists = await vehicleReadRepository.ExistsByEngineNumberAsync(
                            engine,
                            resolvedVariantId.Value,
                            resolvedColorId,
                            cancellationToken)
                            .ConfigureAwait(false);
                        if (isEngineExists)
                        {
                            return Error.BadRequest($"Số máy {engine} đã tồn tại trong hệ thống.", "Products");
                        }
                    }
                }
            }
        }

        var inventoryReceipt = request.Adapt<InventoryReceiptEntity>();
        if (!string.IsNullOrEmpty(inventoryReceipt.Notes))
        {
            inventoryReceipt.Notes = HtmlTagRegex().Replace(inventoryReceipt.Notes, string.Empty);
        }
        inventoryReceipt.StatusId = Domain.Constants.InventoryReceiptStatus.Draft;
        inventoryReceipt.CreatedBy = currentUserContext?.GetUserId();

        var inventoryReceiptInfos = new List<InventoryReceiptInfoEntity>();
        foreach (var p in request.Products)
        {
            var resolvedVariantId = p.PurchaseOrderItemId.HasValue &&
                    poItemsDict.TryGetValue(p.PurchaseOrderItemId.Value, out var poItem)
                ? poItem.ProductVariantId
                : (int?)null;

            var resolvedColorId = p.PurchaseOrderItemId.HasValue &&
                    poItemsDict.TryGetValue(p.PurchaseOrderItemId.Value, out var poItem2)
                ? poItem2.ProductVariantColorId
                : (int?)null;

            var inventoryReceiptInfo = p.Adapt<InventoryReceiptInfoEntity>();
            inventoryReceiptInfo.RemainingCount = p.Count ?? 0;

            if (resolvedVariantId.HasValue && variantMap.TryGetValue(resolvedVariantId.Value, out var variant))
            {
                var managementType = variant.Product?.ProductCategory?.ManagementType;
                if (string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase) &&
                    p.Vehicles != null)
                {
                    inventoryReceiptInfo.Vehicles = [.. p.Vehicles
                        .Select(
                            v => new Vehicle
                            {
                                VinNumber = v.VinNumber.Trim(),
                                EngineNumber = v.EngineNumber.Trim(),
                                LicensePlate = string.Empty,
                                ProductVariantId = variant.Id,
                                ProductVariantColorId = resolvedColorId,
                                LeadId = null,
                                PurchaseDate = DateTimeOffset.UtcNow,
                                IsActive = true,
                                Status = VehicleStatus.Available
                            })];
                }
            }
            inventoryReceiptInfos.Add(inventoryReceiptInfo);
        }

        inventoryReceipt.InventoryReceiptInfos = inventoryReceiptInfos;
        insertRepository.Add(inventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await readRepository.GetByIdWithDetailsAsync(inventoryReceipt.Id, cancellationToken)
            .ConfigureAwait(false);
        return created!.Adapt<InventoryReceiptDetailResponse>();
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
