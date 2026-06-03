using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.SupplierContract;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Constants.Input;
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

public sealed class CreateInputCommandHandler(
    IInputInsertRepository insertRepository,
    IInputReadRepository readRepository,
    ISupplierReadRepository supplierRepository,
    IProductVariantReadRepository variantRepository,
    IVehicleReadRepository vehicleReadRepository,
    ISupplierContractReadRepository supplierContractRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateInputCommand, Result<InputDetailResponse?>>
{
    public async Task<Result<InputDetailResponse?>> Handle(
        CreateInputCommand request,
        CancellationToken cancellationToken)
    {
        if (request.SupplierId.HasValue)
        {
            var supplier = await supplierRepository.GetByIdAsync(
                request.SupplierId.Value,
                cancellationToken,
                DataFetchMode.ActiveOnly).ConfigureAwait(false);
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
        foreach (var product in request.Products)
        {
            if (product.ProductVarientId.HasValue)
            {
                var variants = await variantRepository.GetByIdAsync(
                    [product.ProductVarientId.Value],
                    cancellationToken,
                    DataFetchMode.ActiveOnly).ConfigureAwait(false);
                var variant = variants.FirstOrDefault();
                if (variant is null)
                {
                    return Error.BadRequest(
                        $"Biến thể sản phẩm {product.ProductVarientId} không tồn tại hoặc đã bị xóa.",
                        "Products");
                }
                var colorValidation = ValidateVariantColor(variant, product.ProductVarientColorId);
                if (colorValidation is not null)
                {
                    return colorValidation;
                }
                variantMap[product.ProductVarientId.Value] = variant;

                var managementType = variant.Product?.ProductCategory?.ManagementType;
                if (string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase))
                {
                    if (product.Vehicles == null || product.Vehicles.Count != (product.Count ?? 0))
                    {
                        return Error.BadRequest(
                            $"Danh sách xe (Vehicles) phải có đúng {product.Count ?? 0} phần tử cho sản phẩm quản lý theo số khung.",
                            "Products");
                    }
                    var uniqueVins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var uniqueEngines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                        if (!uniqueVins.Add(vin))
                        {
                            return Error.BadRequest($"Số khung trùng lặp trong yêu cầu: {vin}", "Products");
                        }
                        if (!uniqueEngines.Add(engine))
                        {
                            return Error.BadRequest($"Số máy trùng lặp trong yêu cầu: {engine}", "Products");
                        }
                        var isVinExists = await vehicleReadRepository.ExistsByVinAsync(vin, cancellationToken).ConfigureAwait(false);
                        if (isVinExists)
                        {
                            return Error.BadRequest($"Số khung (VIN) {vin} đã tồn tại trong hệ thống.", "Products");
                        }
                        var isEngineExists = await vehicleReadRepository.ExistsByEngineNumberAsync(
                            engine, cancellationToken).ConfigureAwait(false);
                        if (isEngineExists)
                        {
                            return Error.BadRequest($"Số máy {engine} đã tồn tại trong hệ thống.", "Products");
                        }
                    }
                }
            }
        }

        // CreditGuard: Block if order value exceeds active contract credit limit
        // Auto-fill wholesale prices from contract SKU table
        if (request.SupplierId.HasValue)
        {
            var activeContract = await supplierContractRepository
                .GetActiveContractBySupplierIdAsync(request.SupplierId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (activeContract != null && activeContract.CreditLimit.HasValue)
            {
                var orderValue = request.Products.Sum(p => (p.Price ?? 0) * (p.Count ?? 0));
                if (orderValue > activeContract.CreditLimit.Value)
                {
                    return Error.BadRequest(
                        $"Vượt hạn mức công nợ! Giá trị đơn nhập ({FormatCurrency(orderValue)}) vượt quá hạn mức tín dụng ({FormatCurrency(activeContract.CreditLimit.Value)}) của hợp đồng {activeContract.ContractNumber}.",
                        "CreditLimit");
                }

                // Auto-fill giá nhập sỉ từ hợp đồng cho các SKU thuộc bảng giá
                foreach (var product in request.Products)
                {
                    if (product.ProductVarientId.HasValue && variantMap.ContainsKey(product.ProductVarientId.Value))
                    {
                        var contractItem = activeContract.ContractItems
                            .FirstOrDefault(ci => ci.ProductVariantId == product.ProductVarientId.Value);
                        if (contractItem != null && !product.Price.HasValue)
                        {
                            product.Price = contractItem.WholesalePrice;
                        }
                    }
                }
            }
        }

        var input = request.Adapt<InputEntity>();
        if (!string.IsNullOrEmpty(input.Notes))
        {
            input.Notes = Regex.Replace(input.Notes, "<.*?>", string.Empty);
        }
        input.StatusId = InputStatus.Working;

        var inputInfos = new List<InputInfoEntity>();
        foreach (var p in request.Products)
        {
            var inputInfo = p.Adapt<InputInfoEntity>();
            inputInfo.RemainingCount = p.Count ?? 0;
            inputInfo.ProductVariantColorId = p.ProductVarientColorId;

            if (p.ProductVarientId.HasValue && variantMap.TryGetValue(p.ProductVarientId.Value, out var variant))
            {
                var managementType = variant.Product?.ProductCategory?.ManagementType;
                if (string.Equals(managementType, "vin_number", StringComparison.OrdinalIgnoreCase) && p.Vehicles != null)
                {
                    inputInfo.Vehicles = p.Vehicles
                        .Select(v => new Vehicle
                        {
                            VinNumber = v.VinNumber.Trim(),
                            EngineNumber = v.EngineNumber.Trim(),
                            LicensePlate = v.LicensePlate?.Trim() ?? string.Empty,
                            ProductId = variant.ProductId,
                            LeadId = null,
                            PurchaseDate = DateTimeOffset.UtcNow,
                            IsActive = true
                        })
                        .ToList();
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

    private static string FormatCurrency(decimal? value) =>
        string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:N0} VND", value ?? 0);
}
