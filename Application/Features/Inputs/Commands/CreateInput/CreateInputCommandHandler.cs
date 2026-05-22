using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Constants.Input;
using Domain.Constants.Order;
using Mapster;
using MediatR;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using InputEntity = Domain.Entities.Input;
using InputInfoEntity = Domain.Entities.InputInfo;
using ProductVariant = Domain.Entities.ProductVariant;
using Vehicle = Domain.Entities.Vehicle;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed partial class CreateInputCommandHandler(
    IInputInsertRepository insertRepository,
    IInputReadRepository readRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IVehicleReadRepository vehicleReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateInputCommand, Result<InputDetailResponse?>>
{
    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlTagRegex();

    public async Task<Result<InputDetailResponse?>> Handle(
        CreateInputCommand request,
        CancellationToken cancellationToken)
    {
        if (request.SupplierId.HasValue)
        {
            var supplier = await supplierRepository.GetByIdAsync(
                request.SupplierId.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly)
                .ConfigureAwait(false);
            if (supplier is null)
            {
                return Error.NotFound($"Nhà cung cấp {request.SupplierId} không tồn tại hoặc đã bị xóa.", "SupplierId");
            }
            if (string.Compare(supplier.StatusId, SupplierStatus.Active) != 0)
            {
                return Error.BadRequest($"Nhà cung cấp {supplier.Name} không ở trạng thái 'active'.", "SupplierId");
            }
        }
        var variantMap = new Dictionary<int, ProductVariant>();
        var uniqueVins = new HashSet<(string Vin, int ProductVariantId, int? ProductVariantColorId)>();
        var uniqueEngines = new HashSet<(string Engine, int ProductVariantId, int? ProductVariantColorId)>();
        foreach (var product in request.Products)
        {
            if (product.ProductVariantId.HasValue)
            {
                var variants = await variantRepository.GetByIdAsync(
                    [product.ProductVariantId.Value],
                    cancellationToken,
                    DataFetchMode.ActiveOnly)
                    .ConfigureAwait(false);
                var variant = variants.FirstOrDefault();
                if (variant is null)
                {
                    return Error.BadRequest(
                        $"Biến thể sản phẩm {product.ProductVariantId} không tồn tại hoặc đã bị xóa.",
                        "Products");
                }
                var colorValidation = ValidateVariantColor(variant, product.ProductVariantColorId);
                if (colorValidation is not null)
                {
                    return colorValidation;
                }
                variantMap[product.ProductVariantId.Value] = variant;
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
                        var normalizedVinKey = (vin.ToUpperInvariant(), product.ProductVariantId.Value, product.ProductVariantColorId);
                        if (!uniqueVins.Add(normalizedVinKey))
                        {
                            return Error.BadRequest($"Số khung trùng lặp trong yêu cầu: {vin}", "Products");
                        }
                        var normalizedEngineKey = (engine.ToUpperInvariant(), product.ProductVariantId.Value, product.ProductVariantColorId);
                        if (!uniqueEngines.Add(normalizedEngineKey))
                        {
                            return Error.BadRequest($"Số máy trùng lặp trong yêu cầu: {engine}", "Products");
                        }
                        var isVinExists = await vehicleReadRepository.ExistsByVinAsync(
                            vin,
                            product.ProductVariantId.Value,
                            product.ProductVariantColorId,
                            cancellationToken).ConfigureAwait(false);
                        if (isVinExists)
                        {
                            return Error.BadRequest($"Số khung (VIN) {vin} đã tồn tại trong hệ thống.", "Products");
                        }
                        var isEngineExists = await vehicleReadRepository.ExistsByEngineNumberAsync(
                            engine,
                            product.ProductVariantId.Value,
                            product.ProductVariantColorId,
                            cancellationToken).ConfigureAwait(false);
                        if (isEngineExists)
                        {
                            return Error.BadRequest($"Số máy {engine} đã tồn tại trong hệ thống.", "Products");
                        }
                    }
                }
            }
        }

        var input = request.Adapt<InputEntity>();
        if (!string.IsNullOrEmpty(input.Notes))
        {
            input.Notes = HtmlTagRegex().Replace(input.Notes, string.Empty);
        }
        input.StatusId = InputStatus.Working;
        var inputInfos = new List<InputInfoEntity>();
        foreach (var p in request.Products)
        {
            var inputInfo = p.Adapt<InputInfoEntity>();
            inputInfo.RemainingCount = p.Count ?? 0;
            inputInfo.ProductVariantColorId = p.ProductVariantColorId;
            if (p.ProductVariantId.HasValue && variantMap.TryGetValue(p.ProductVariantId.Value, out var variant))
            {
                var managementType = variant.Product?.ProductCategory?.ManagementType;
                if (string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase) &&
                    p.Vehicles != null)
                {
                    inputInfo.Vehicles = [.. p.Vehicles
                        .Select(
                            v => new Vehicle
                            {
                                VinNumber = v.VinNumber.Trim(),
                                EngineNumber = v.EngineNumber.Trim(),
                                LicensePlate = string.Empty,
                                ProductVariantId = variant.Id,
                                ProductVariantColorId = p.ProductVariantColorId,
                                LeadId = null,
                                PurchaseDate = DateTimeOffset.UtcNow,
                                IsActive = true,
                                Status = VehicleStatus.Available
                            })];
                }
            }
            inputInfos.Add(inputInfo);
        }
        input.InputInfos = inputInfos;
        insertRepository.Add(input);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var created = await readRepository.GetByIdWithDetailsAsync(input.Id, cancellationToken).ConfigureAwait(false);
        return created!.Adapt<InputDetailResponse>();
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
