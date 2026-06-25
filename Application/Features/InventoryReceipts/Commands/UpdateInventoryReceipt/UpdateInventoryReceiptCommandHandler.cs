using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Constants.PurchaseRequest;
using Domain.Entities;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;

public sealed partial class UpdateInventoryReceiptCommandHandler(
    IInventoryReceiptReadRepository readRepository,
    IInventoryReceiptUpdateRepository updateRepository,
    IInventoryReceiptDeleteRepository deleteRepository,
    IPurchaseRequestReadRepository prReadRepository,
    IProductVariantReadRepository variantRepository,
    IPermissionReadRepository permissionRepository,
    IVehicleUpdateRepository vehicleUpdateRepository,
    IVehicleReadRepository vehicleReadRepository,
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
        if (Domain.Constants.InventoryReceipt.InventoryReceiptStatus.IsCannotEdit(inventoryReceipt.StatusId))
        {
            return Error.BadRequest("Khi đã phê duyệt hoặc từ chối thì không được sửa phiếu.", "StatusId");
        }
        Guid userId = currentUserContext.GetUserId();
        if (string.Equals(
            inventoryReceipt.StatusId,
            Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Sent,
            StringComparison.OrdinalIgnoreCase))
        {
            var hasApprovePermission = await permissionRepository.CheckUserPermissionsAsync(
                userId,
                [Domain.Constants.Permission.Permissions.InventoryReceipts.ApproveReject],
                cancellationToken)
                .ConfigureAwait(false);
            if (!hasApprovePermission)
            {
                return Error.BadRequest(
                    "Chỉ người có quyền phê duyệt/từ chối mới được sửa phiếu nhập ở trạng thái đã gửi.",
                    "StatusId");
            }
        } else if (!string.Equals(
            inventoryReceipt.StatusId,
            Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Draft,
            StringComparison.OrdinalIgnoreCase))
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
        var existingInfoDict = inventoryReceipt.InventoryReceiptInfos.ToDictionary(ii => ii.Id);
        var purchaseRequestId = request.PurchaseRequestId ?? inventoryReceipt.PurchaseRequestId;
        PurchaseRequest? pr = null;
        if (purchaseRequestId.HasValue)
        {
            pr = await prReadRepository.GetByIdWithDetailsAsync(purchaseRequestId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (pr is null)
            {
                return Error.NotFound(
                    $"Yêu cầu mua hàng {purchaseRequestId} không tồn tại hoặc đã bị xóa.",
                    "PurchaseRequestId");
            }
            if (!string.Equals(pr.Status, PurchaseRequestStatus.Approve, StringComparison.OrdinalIgnoreCase))
            {
                return Error.BadRequest(
                    $"Yêu cầu mua hàng {purchaseRequestId} chưa được phê duyệt.",
                    "PurchaseRequestId");
            }
        }
        var prItemsDict = pr != null ? pr.PurchaseRequestItems.ToDictionary(x => x.Id) : [];
        if (prItemsDict.Count == 0 && request.Products.Any(p => p.PurchaseRequestItemId.HasValue))
        {
            var prItemIds = request.Products
                .Where(p => p.PurchaseRequestItemId.HasValue)
                .Select(p => p.PurchaseRequestItemId!.Value)
                .Distinct();
            var prItems = await prReadRepository.GetItemsByIdsAsync(prItemIds, cancellationToken).ConfigureAwait(false);
            prItemsDict = prItems.ToDictionary(x => x.Id);
        }
        var variantIds = new List<int>();
        foreach (var productRequest in request.Products)
        {
            int? purchaseRequestItemId = null;
            if (productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if (existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    purchaseRequestItemId = productRequest.PurchaseRequestItemId ?? existingInfo.PurchaseRequestItemId;
                }
            } else
            {
                purchaseRequestItemId = productRequest.PurchaseRequestItemId;
            }
            if (purchaseRequestItemId.HasValue && prItemsDict.TryGetValue(purchaseRequestItemId.Value, out var prItem))
            {
                variantIds.Add(prItem.ProductVariantId);
            }
        }
        variantIds = variantIds.Distinct().ToList();
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
                int? purchaseRequestItemId = null;
                if (product.Id.HasValue &&
                    product.Id > 0 &&
                    existingInfoDict.TryGetValue(product.Id.Value, out var existingInfo))
                {
                    purchaseRequestItemId = product.PurchaseRequestItemId ?? existingInfo.PurchaseRequestItemId;
                } else
                {
                    purchaseRequestItemId = product.PurchaseRequestItemId;
                }
                PurchaseRequestItem? prItem = null;
                var resolvedVariantId = purchaseRequestItemId.HasValue &&
                        prItemsDict.TryGetValue(purchaseRequestItemId.Value, out prItem)
                    ? prItem.ProductVariantId
                    : (int?)null;
                var resolvedColorId = purchaseRequestItemId.HasValue &&
                        prItemsDict.TryGetValue(purchaseRequestItemId.Value, out var prItem2)
                    ? prItem2.ProductVariantColorId
                    : (int?)null;
                if (resolvedVariantId.HasValue)
                {
                    if (prItem != null)
                    {
                        var occupiedQty = prItem.InventoryReceiptInfos
                            .Where(
                                ii => ii.DeletedAt == null &&
                                    ii.InventoryReceiptId != request.Id &&
                                    ii.InventoryReceipt != null &&
                                    ii.InventoryReceipt.DeletedAt == null &&
                                    (string.Equals(
                                            ii.InventoryReceipt.StatusId,
                                            Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve,
                                            StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(
                                            ii.InventoryReceipt.StatusId,
                                            Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Sent,
                                            StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(
                                            ii.InventoryReceipt.StatusId,
                                            Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Draft,
                                            StringComparison.OrdinalIgnoreCase)))
                            .Sum(ii => ii.Count ?? 0);
                        var remainingAllowed = prItem.Quantity - occupiedQty;
                        var requestedQty = product.Count ?? 0;
                        if (requestedQty > remainingAllowed)
                        {
                            var productName = prItem.ProductVariant?.Product?.Name ??
                                $"Biến thể #{prItem.ProductVariantId}";
                            return Error.BadRequest(
                                $"Số lượng nhập ({requestedQty}) cho sản phẩm '{productName}' vượt quá số lượng còn lại được phép nhập từ yêu cầu mua hàng ({remainingAllowed}).",
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
        var oldPurchaseRequestId = inventoryReceipt.PurchaseRequestId;
        var oldNotes = inventoryReceipt.Notes;
        request.Adapt(inventoryReceipt);
        if (!request.PurchaseRequestId.HasValue)
        {
            inventoryReceipt.PurchaseRequestId = oldPurchaseRequestId;
        }
        if (request.Notes == null)
        {
            inventoryReceipt.Notes = oldNotes;
        }
        if (!string.IsNullOrEmpty(inventoryReceipt.Notes))
        {
            inventoryReceipt.Notes = HtmlTagRegex().Replace(inventoryReceipt.Notes, string.Empty);
        }
        var requestInfoDict = request.Products.Where(p => p.Id.HasValue && p.Id > 0).ToDictionary(p => p.Id!.Value);
        var toDelete = inventoryReceipt.InventoryReceiptInfos.Where(ii => !requestInfoDict.ContainsKey(ii.Id)).ToList();
        foreach (var info in toDelete)
        {
            deleteRepository.DeleteInventoryReceiptInfo(info);
            inventoryReceipt.InventoryReceiptInfos.Remove(info);
        }
        var config = new TypeAdapterConfig();
        config.NewConfig<UpdateInventoryReceiptInfoRequest, InventoryReceiptInfo>().Ignore(dest => dest.Vehicles);
        var vehicleAuditLogs = new List<Domain.Entities.VehicleAuditLog>();
        foreach (var productRequest in request.Products)
        {
            InventoryReceiptInfo? existingInfo = null;
            if (productRequest.Id.HasValue && productRequest.Id > 0)
            {
                existingInfoDict.TryGetValue(productRequest.Id.Value, out existingInfo);
            }
            int? purchaseRequestItemId = existingInfo != null
                ? (productRequest.PurchaseRequestItemId ?? existingInfo.PurchaseRequestItemId)
                : productRequest.PurchaseRequestItemId;
            var resolvedColorId = purchaseRequestItemId.HasValue &&
                    prItemsDict.TryGetValue(purchaseRequestItemId.Value, out var prItem)
                ? prItem.ProductVariantColorId
                : (int?)null;
            var resolvedVariantId = purchaseRequestItemId.HasValue &&
                    prItemsDict.TryGetValue(purchaseRequestItemId.Value, out var prItemV)
                ? prItemV.ProductVariantId
                : (int?)null;
            if (existingInfo != null)
            {
                var oldPurchaseRequestItemId = existingInfo.PurchaseRequestItemId;
                productRequest.Adapt(existingInfo, config);
                if (!productRequest.PurchaseRequestItemId.HasValue)
                {
                    existingInfo.PurchaseRequestItemId = oldPurchaseRequestItemId;
                }
                if (productRequest.Count.HasValue)
                {
                    existingInfo.RemainingCount = productRequest.Count.Value;
                }
                var variant = variantsList.FirstOrDefault(v => v.Id == resolvedVariantId);
                await SyncVehicleIdentifiersAsync(
                    existingInfo,
                    productRequest,
                    variant,
                    resolvedColorId,
                    vehicleUpdateRepository,
                    vehicleReadRepository,
                    userId,
                    vehicleAuditLogs,
                    cancellationToken)
                    .ConfigureAwait(false);
            } else
            {
                var newInfo = productRequest.Adapt<InventoryReceiptInfo>(config);
                newInfo.RemainingCount = newInfo.Count ?? 0;
                var variant = variantsList.FirstOrDefault(v => v.Id == resolvedVariantId);
                await SyncVehicleIdentifiersAsync(
                    newInfo,
                    productRequest,
                    variant,
                    resolvedColorId,
                    vehicleUpdateRepository,
                    vehicleReadRepository,
                    userId,
                    vehicleAuditLogs,
                    cancellationToken)
                    .ConfigureAwait(false);
                inventoryReceipt.InventoryReceiptInfos.Add(newInfo);
            }
        }
        updateRepository.Update(inventoryReceipt);
        if (vehicleAuditLogs.Any())
        {
            await vehicleUpdateRepository.InsertAuditLogsAsync(vehicleAuditLogs, cancellationToken).ConfigureAwait(false);
        }
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

    private static async Task SyncVehicleIdentifiersAsync(
        InventoryReceiptInfo inventoryReceiptInfo,
        UpdateInventoryReceiptInfoRequest productRequest,
        ProductVariant? variant,
        int? resolvedColorId,
        IVehicleUpdateRepository vehicleUpdateRepository,
        IVehicleReadRepository vehicleReadRepository,
        Guid? currentUserId,
        List<Domain.Entities.VehicleAuditLog> vehicleAuditLogs,
        CancellationToken cancellationToken)
    {
        var managementType = variant?.Product?.ProductCategory?.ManagementType;
        if (!string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var vehicle in inventoryReceiptInfo.Vehicles.ToList())
            {
                vehicleAuditLogs.Add(new Domain.Entities.VehicleAuditLog
                {
                    Vehicle = vehicle,
                    Action = "Delete",
                    ChangedById = currentUserId,
                    ChangedAt = DateTimeOffset.UtcNow,
                    OldVinNumber = vehicle.VinNumber,
                    OldEngineNumber = vehicle.EngineNumber
                });
                vehicleUpdateRepository.Remove(vehicle);
                inventoryReceiptInfo.Vehicles.Remove(vehicle);
            }
            return;
        }
        var requestedVehicles = productRequest.Vehicles ?? [];
        var existingVehicles = inventoryReceiptInfo.Vehicles.ToDictionary(v => v.Id);
        var requestedIds = requestedVehicles.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet();
        foreach (var existingVehicle in inventoryReceiptInfo.Vehicles.ToList())
        {
            if (!requestedIds.Contains(existingVehicle.Id))
            {
                vehicleAuditLogs.Add(new Domain.Entities.VehicleAuditLog
                {
                    Vehicle = existingVehicle,
                    Action = "Delete",
                    ChangedById = currentUserId,
                    ChangedAt = DateTimeOffset.UtcNow,
                    OldVinNumber = existingVehicle.VinNumber,
                    OldEngineNumber = existingVehicle.EngineNumber
                });
                vehicleUpdateRepository.Remove(existingVehicle);
                inventoryReceiptInfo.Vehicles.Remove(existingVehicle);
            }
        }
        var updatedVehicles = new List<Vehicle>();
        foreach (var vehicleRequest in requestedVehicles)
        {
            Vehicle? vehicle = null;
            if (vehicleRequest.Id.HasValue)
            {
                if (existingVehicles.TryGetValue(vehicleRequest.Id.Value, out var existingVehicle))
                {
                    vehicle = existingVehicle;
                } else
                {
                    vehicle = await vehicleReadRepository.GetByIdAsync(vehicleRequest.Id.Value, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    LicensePlate = string.Empty,
                    ProductVariantId = variant?.Id,
                    ProductVariantColorId = resolvedColorId,
                    LeadId = null,
                    PurchaseDate = DateTimeOffset.UtcNow,
                    IsActive = true,
                    Status = VehicleStatus.Available
                };
                vehicleAuditLogs.Add(new Domain.Entities.VehicleAuditLog
                {
                    Vehicle = vehicle,
                    Action = "Add",
                    ChangedById = currentUserId,
                    ChangedAt = DateTimeOffset.UtcNow,
                    NewVinNumber = vehicleRequest.VinNumber.Trim(),
                    NewEngineNumber = vehicleRequest.EngineNumber.Trim()
                });
            }
            else
            {
                if (vehicle.VinNumber != vehicleRequest.VinNumber.Trim() || vehicle.EngineNumber != vehicleRequest.EngineNumber.Trim())
                {
                    vehicleAuditLogs.Add(new Domain.Entities.VehicleAuditLog
                    {
                        Vehicle = vehicle,
                        Action = "Update",
                        ChangedById = currentUserId,
                        ChangedAt = DateTimeOffset.UtcNow,
                        OldVinNumber = vehicle.VinNumber,
                        OldEngineNumber = vehicle.EngineNumber,
                        NewVinNumber = vehicleRequest.VinNumber.Trim(),
                        NewEngineNumber = vehicleRequest.EngineNumber.Trim()
                    });
                }
            }
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
