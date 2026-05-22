using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;

using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using System.Text.RegularExpressions;

namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed class UpdateInputCommandHandler(
    IInputReadRepository readRepository,
    IInputUpdateRepository updateRepository,
    IInputDeleteRepository deleteRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateInputCommand, Result<InputDetailResponse?>>
{
    public async Task<Result<InputDetailResponse?>> Handle(
        UpdateInputCommand request,
        CancellationToken cancellationToken)
    {
        var input = await readRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken,
            DataFetchMode.ActiveOnly)
            .ConfigureAwait(false);
        if (input is null)
        {
            return Error.NotFound($"Không tìm thấy phiếu nhập có ID {request.Id}.", "Id");
        }
        if (Domain.Constants.Input.InputStatus.IsCannotEdit(input.StatusId))
        {
            if (request.Products.Count != 0)
            {
                return Error.BadRequest(
                    "Không được chỉnh sửa sản phẩm trong phiếu nhập đã hoàn thành hoặc đã hủy.",
                    "Products");
            }
            if (request.SupplierId != null)
            {
                return Error.BadRequest(
                    "Không được chỉnh nhà cung cấp trong phiếu nhập đã hoàn thành hoặc đã hủy.",
                    "Products");
            }
        }
        if (request.SupplierId.HasValue && request.SupplierId != input.SupplierId)
        {
            var supplier = await supplierRepository.GetByIdAsync(
                request.SupplierId.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            if (supplier is null || string.Compare(supplier.StatusId, Domain.Constants.SupplierStatus.Active) != 0)
            {
                return Error.BadRequest("Nhà cung cấp không hợp lệ hoặc không còn hoạt động.", "SupplierId");
            }
        }
        var variantIds = request.Products
            .Where(p => p.ProductVarientId.HasValue)
            .Select(p => p.ProductVarientId!.Value)
            .Distinct()
            .ToList();
        var variantsList = new List<ProductVariant>();
        if (variantIds.Count > 0)
        {
            var variants = await variantRepository.GetByIdAsync(variantIds, cancellationToken, DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            variantsList = variants.ToList();
            if (variantsList.Count != variantIds.Count)
            {
                var foundIds = variantsList.Select(v => v.Id).ToList();
                var missingIds = variantIds.Except(foundIds).ToList();
                return Error.NotFound(
                    $"Không tìm thấy {missingIds.Count} s?n ph?m: {string.Join(", ", missingIds)}",
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
            foreach (var product in request.Products.Where(p => p.ProductVarientId.HasValue))
            {
                var variant = variantsList.First(v => v.Id == product.ProductVarientId!.Value);
                var colorValidation = ValidateVariantColor(variant, product.ProductVarientColorId);
                if (colorValidation is not null)
                {
                    return colorValidation;
                }
                var vehicleValidation = ValidateVehicleIdentifiers(variant, product);
                if (vehicleValidation is not null)
                {
                    return vehicleValidation;
                }
            }
        }
        request.Adapt(input);
        if (!string.IsNullOrEmpty(input.Notes))
        {
            input.Notes = Regex.Replace(input.Notes, "<.*?>", string.Empty);
        }
        if (string.Equals(
            request.StatusId,
            Domain.Constants.Input.InputStatus.Finish,
            StringComparison.OrdinalIgnoreCase))
        {
            input.InputDate = DateTimeOffset.UtcNow;
            input.ConfirmedBy = request.CurrentUserId;
        }
        var existingInfoDict = input.InputInfos.ToDictionary(ii => ii.Id);
        var requestInfoDict = request.Products.Where(p => p.Id.HasValue && p.Id > 0).ToDictionary(p => p.Id!.Value);
        var toDelete = input.InputInfos.Where(ii => !requestInfoDict.ContainsKey(ii.Id)).ToList();
        foreach (var info in toDelete)
        {
            deleteRepository.DeleteInputInfo(info);
            input.InputInfos.Remove(info);
        }
        foreach (var productRequest in request.Products)
        {
            if (productRequest.Id.HasValue && productRequest.Id > 0)
            {
                if (existingInfoDict.TryGetValue(productRequest.Id.Value, out var existingInfo))
                {
                    productRequest.Adapt(existingInfo);
                    if (productRequest.Count.HasValue)
                    {
                        existingInfo.RemainingCount = productRequest.Count.Value;
                    }
                    var variant = variantsList.FirstOrDefault(v => v.Id == productRequest.ProductVarientId);
                    SyncVehicleIdentifiers(existingInfo, productRequest, variant);
                }
            } else
            {
                var newInfo = productRequest.Adapt<InputInfo>();
                newInfo.RemainingCount = newInfo.Count ?? 0;
                var variant = variantsList.FirstOrDefault(v => v.Id == productRequest.ProductVarientId);
                SyncVehicleIdentifiers(newInfo, productRequest, variant);
                input.InputInfos.Add(newInfo);
            }
        }
        updateRepository.Update(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var updated = await readRepository.GetByIdWithDetailsAsync(input.Id, cancellationToken).ConfigureAwait(false);
        return updated!.Adapt<InputDetailResponse>();
    }

    private static Error? ValidateVariantColor(ProductVariant variant, int? productVarientColorId)
    {
        if (variant.ProductVariantColors.Count == 0)
        {
            return productVarientColorId.HasValue
                ? Error.BadRequest("Biến thể sản phẩm này không có màu sắc để chọn.", "ProductVarientColorId")
                : null;
        }
        if (!productVarientColorId.HasValue || productVarientColorId <= 0)
        {
            return Error.BadRequest(
                "Biến thể sản phẩm có màu sắc, ProductVarientColorId là bắt buộc.",
                "ProductVarientColorId");
        }
        return variant.ProductVariantColors.Any(c => c.Id == productVarientColorId.Value)
            ? null
            : Error.BadRequest("ProductVarientColorId không thuộc biến thể sản phẩm đã chọn.", "ProductVarientColorId");
    }

    private static Error? ValidateVehicleIdentifiers(
        ProductVariant variant,
        Application.ApiContracts.Input.Requests.UpdateInputInfoRequest product)
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
        var uniqueVins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var uniqueEngines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
            if (!uniqueVins.Add(vin))
            {
                return Error.BadRequest($"Số khung trùng lặp trong yêu cầu: {vin}", "Products");
            }
            if (!uniqueEngines.Add(engine))
            {
                return Error.BadRequest($"Số máy trùng lặp trong yêu cầu: {engine}", "Products");
            }
        }
        return null;
    }

    private static void SyncVehicleIdentifiers(
        InputInfo inputInfo,
        Application.ApiContracts.Input.Requests.UpdateInputInfoRequest productRequest,
        ProductVariant? variant)
    {
        var managementType = variant?.Product?.ProductCategory?.ManagementType;
        if (!string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
        {
            inputInfo.Vehicles.Clear();
            return;
        }
        var requestedVehicles = productRequest.Vehicles ?? [];
        var existingVehicles = inputInfo.Vehicles.ToDictionary(v => v.Id);
        var updatedVehicles = new List<Vehicle>();
        foreach (var vehicleRequest in requestedVehicles)
        {
            var vehicle = vehicleRequest.Id.HasValue &&
                existingVehicles.TryGetValue(vehicleRequest.Id.Value, out var existingVehicle)
                    ? existingVehicle
                    : new Vehicle
                    {
                        LicensePlate = string.Empty,
                        ProductId = variant?.ProductId,
                        LeadId = null,
                        PurchaseDate = DateTimeOffset.UtcNow,
                        IsActive = true
                    };
            vehicle.VinNumber = vehicleRequest.VinNumber.Trim();
            vehicle.EngineNumber = vehicleRequest.EngineNumber.Trim();
            vehicle.ProductId = variant?.ProductId;
            updatedVehicles.Add(vehicle);
        }
        inputInfo.Vehicles.Clear();
        foreach (var vehicle in updatedVehicles)
        {
            inputInfo.Vehicles.Add(vehicle);
        }
    }
}
