using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseOrder;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;
using Application.Interfaces.Repositories.Permission;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;

public sealed partial class UpdateInventoryReceiptCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    IInventoryReceiptDeleteRepository deleteRepository,
    IPurchaseOrderReadRepository poReadRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IPermissionReadRepository permissionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext) : IRequestHandler<UpdateInventoryReceiptCommand, Result<InventoryReceiptDetailResponse?>>
{
    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlTagRegex();

    public async Task<Result<InventoryReceiptDetailResponse?>> Handle(
        UpdateInventoryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var inventoryReceipt = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (inventoryReceipt is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id");
        }
        if (Domain.Constants.InventoryReceiptStatus.IsCannotEdit(inventoryReceipt.StatusId))
        {
            return Error.BadRequest("Khi đã phê duyệt hoặc từ chối thì không được sửa phiếu.", "StatusId");
        }

        Guid userId = currentUserContext.GetUserId();
        if (string.Equals(inventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Sent, StringComparison.OrdinalIgnoreCase))
        {
            var hasApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                userId,
                [Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject],
                cancellationToken)
                .ConfigureAwait(false);
            if (!hasApprovePermission)
            {
                return Error.BadRequest("Chỉ người có quyền phê duyệt/từ chối mới được sửa phiếu nhập ở trạng thái đã gửi.", "StatusId");
            }
        }
        else if (!string.Equals(inventoryReceipt.StatusId, Domain.Constants.InventoryReceiptStatus.Draft, StringComparison.OrdinalIgnoreCase))
        {
            var hasEditOrApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                userId,
                [Domain.Constants.Permission.Permissions.InventoryReceipts.Edit, Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject],
                cancellationToken)
                .ConfigureAwait(false);
            if (!hasEditOrApprovePermission)
            {
                return Error.BadRequest("Bạn không có quyền chỉnh sửa phiếu nhập này.", "StatusId");
            }
        }

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

        var variantIds = request.Products
            .Select(
                p => p.PurchaseOrderItemId.HasValue &&
                        poItemsDict.TryGetValue(p.PurchaseOrderItemId.Value, out var poItem)
                    ? poItem.ProductVariantId
                    : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var variantsList = new List<ProductVariant>();
        if (variantIds.Count > 0)
        {
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            variantsList = [.. variants];
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
                    if (resolvedSupplierId.HasValue)
                    {
                        var supplier = await supplierRepository.GetByIdAsync(
                            resolvedSupplierId.Value,
                            cancellationToken,
                            DataFetchMode.ActiveOnly)
                            .ConfigureAwait(false);
                        if (supplier is null ||
                            string.Compare(supplier.StatusId, Domain.Constants.SupplierStatus.Active) != 0)
                        {
                            return Error.BadRequest(
                                $"Nhà cung cấp với ID {resolvedSupplierId.Value} không tồn tại hoặc không còn hoạt động.",
                                "Products");
                        }
                    }

                    var variant = variantsList.First(v => v.Id == resolvedVariantId.Value);
                    var colorValidation = ValidateVariantColor(variant, resolvedColorId);
                    if (colorValidation is not null)
                    {
                        return colorValidation;
                    }

                    var vehicleValidation = ValidateVehicleIdentifiers(
                        variant,
                        product,
                        resolvedVariantId.Value,
                        resolvedColorId,
                        uniqueVins,
                        uniqueEngines);
                    if (vehicleValidation is not null)
                    {
                        return vehicleValidation;
                    }
                }
            }
        }

        request.Adapt(inventoryReceipt);
        if (!string.IsNullOrEmpty(inventoryReceipt.Notes))
        {
            inventoryReceipt.Notes = HtmlTagRegex().Replace(inventoryReceipt.Notes, string.Empty);
        }

        var existingInfoDict = inventoryReceipt.InventoryReceiptInfos.ToDictionary(ii => ii.Id);
        var requestInfoDict = request.Products.Where(p => p.Id.HasValue && p.Id > 0).ToDictionary(p => p.Id!.Value);
        
        var toDelete = inventoryReceipt.InventoryReceiptInfos.Where(ii => !requestInfoDict.ContainsKey(ii.Id)).ToList();
        foreach (var info in toDelete)
        {
            deleteRepository.DeleteInventoryReceiptInfo(info);
            inventoryReceipt.InventoryReceiptInfos.Remove(info);
        }

        foreach (var productRequest in request.Products)
        {
            var resolvedColorId = productRequest.PurchaseOrderItemId.HasValue &&
                    poItemsDict.TryGetValue(productRequest.PurchaseOrderItemId.Value, out var poItem)
                ? poItem.ProductVariantColorId
                : (int?)null;

            var resolvedVariantId = productRequest.PurchaseOrderItemId.HasValue &&
                    poItemsDict.TryGetValue(productRequest.PurchaseOrderItemId.Value, out var poItemV)
                ? poItemV.ProductVariantId
                : (int?)null;

            if (productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if (existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    productRequest.Adapt(existingInfo);
                    if (productRequest.Count.HasValue)
                    {
                        existingInfo.RemainingCount = productRequest.Count.Value;
                    }
                    var variant = variantsList.FirstOrDefault(v => v.Id == resolvedVariantId);
                    SyncVehicleIdentifiers(existingInfo, productRequest, variant, resolvedColorId);
                }
            } 
            else
            {
                var newInfo = productRequest.Adapt<InventoryReceiptInfo>();
                newInfo.RemainingCount = newInfo.Count ?? 0;
                var variant = variantsList.FirstOrDefault(v => v.Id == resolvedVariantId);
                SyncVehicleIdentifiers(newInfo, productRequest, variant, resolvedColorId);
                inventoryReceipt.InventoryReceiptInfos.Add(newInfo);
            }
        }

        updateRepository.Update(inventoryReceipt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var updated = await readRepository.GetByIdWithDetailsAsync(inventoryReceipt.Id, cancellationToken)
            .ConfigureAwait(false);
        return updated!.Adapt<InventoryReceiptDetailResponse>();
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

    private static Error? ValidateVehicleIdentifiers(
        ProductVariant variant,
        UpdateInventoryReceiptInfoRequest product,
        int resolvedVariantId,
        int? resolvedColorId,
        HashSet<(string Vin, int ProductVariantId, int? ProductVariantColorId)> uniqueVins,
        HashSet<(string Engine, int ProductVariantId, int? ProductVariantColorId)> uniqueEngines)
    {
        var managementType = variant.Product?.ProductCategory?.ManagementType;
        if (!string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        if (product.Vehicles == null || product.Vehicles.Count != (product.Count ?? 0))
        {
            return Error.BadRequest(
                $"Danh sách xe (Vehicles) phải có đúng {product.Count ?? 0} phần tử cho sản phẩm quản lý theo số khung.",
                "Products");
        }
        foreach (var vehicle in product.Vehicles)
        {
            if (string.IsNullOrWhiteSpace(vehicle.VinNumber) || string.IsNullOrWhiteSpace(vehicle.EngineNumber))
            {
                return Error.BadRequest(
                    "Số khung (VinNumber) và Số máy (EngineNumber) không được để trống.",
                    "Products");
            }
            var vin = vehicle.VinNumber.Trim();
            var engine = vehicle.EngineNumber.Trim();
            var normalizedVinKey = (vin.ToUpperInvariant(), resolvedVariantId, resolvedColorId);
            if (!uniqueVins.Add(normalizedVinKey))
            {
                return Error.BadRequest($"Số khung trùng lặp trong yêu cầu: {vin}", "Products");
            }
            var normalizedEngineKey = (engine.ToUpperInvariant(), resolvedVariantId, resolvedColorId);
            if (!uniqueEngines.Add(normalizedEngineKey))
            {
                return Error.BadRequest($"Số máy trùng lặp trong yêu cầu: {engine}", "Products");
            }
        }
        return null;
    }

    private static void SyncVehicleIdentifiers(
        InventoryReceiptInfo inventoryReceiptInfo,
        UpdateInventoryReceiptInfoRequest productRequest,
        ProductVariant? variant,
        int? resolvedColorId)
    {
        var managementType = variant?.Product?.ProductCategory?.ManagementType;
        if (!string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
        {
            inventoryReceiptInfo.Vehicles.Clear();
            return;
        }
        var requestedVehicles = productRequest.Vehicles ?? [];
        var existingVehicles = inventoryReceiptInfo.Vehicles.ToDictionary(v => v.Id);
        var updatedVehicles = new List<Vehicle>();
        foreach (var vehicleRequest in requestedVehicles)
        {
            var vehicle = vehicleRequest.Id.HasValue &&
                    existingVehicles.TryGetValue(vehicleRequest.Id.Value, out var existingVehicle)
                ? existingVehicle
                : new Vehicle
                {
                    LicensePlate = string.Empty,
                    ProductVariantId = variant?.Id,
                    ProductVariantColorId = resolvedColorId,
                    LeadId = null,
                    PurchaseDate = DateTimeOffset.UtcNow,
                    IsActive = true,
                    Status = VehicleStatus.Available
                };
            vehicle.VinNumber = vehicleRequest.VinNumber.Trim();
            vehicle.EngineNumber = vehicleRequest.EngineNumber.Trim();
            vehicle.ProductVariantId = variant?.Id;
            vehicle.ProductVariantColorId = resolvedColorId;
            if (string.IsNullOrWhiteSpace(vehicle.Status))
            {
                vehicle.Status = VehicleStatus.Available;
            }
            updatedVehicles.Add(vehicle);
        }
        inventoryReceiptInfo.Vehicles.Clear();
        foreach (var vehicle in updatedVehicles)
        {
            inventoryReceiptInfo.Vehicles.Add(vehicle);
        }
    }
}
