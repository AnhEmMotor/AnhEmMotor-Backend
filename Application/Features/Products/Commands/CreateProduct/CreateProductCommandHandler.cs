using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Helper;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;
using Application.Interfaces.Repositories.PredefinedOption;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductQuotations;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Technology.Technology;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductCategoryReadRepository productCategoryReadRepository,
    IBrandReadRepository brandReadRepository,
    IProductVariantReadRepository productVariantReadRepository,
    IOptionValueReadRepository optionValueReadRepository,
    IOptionReadRepository optionReadRepository,
    IPredefinedOptionReadRepository predefinedOptionReadRepository,
    IProductInsertRepository productInsertRepository,
    IProductVariantInsertRepository productVariantInsertRepository,
    IOptionValueInsertRepository optionValueInsertRepository,
    ITechnologyReadRepository technologyReadRepository,
    IUnitOfWork unitOfWork,
    IProductQuotationReadRepository? ProductQuotationReadRepository = null,
    IProductQuotationInsertRepository? ProductQuotationInsertRepository = null,
    IProductQuotationUpdateRepository? ProductQuotationUpdateRepository = null,
    IProductQuotationDeleteRepository? ProductQuotationDeleteRepository = null) : IRequestHandler<CreateProductCommand, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();
        if (request.Variants is null || request.Variants.Count == 0)
        {
            return Result<ProductDetailForManagerResponse?>.Failure(
                Error.BadRequest("Sản phẩm phải có ít nhất một biến thể.", nameof(request.Variants)));
        }
        var category = await productCategoryReadRepository.GetByIdAsync(request.CategoryId!.Value, cancellationToken)
            .ConfigureAwait(false);
        if (category == null)
        {
            errors.Add(
                Error.NotFound(
                    $"Danh mục sản phẩm với ID {request.CategoryId} không tồn tại.",
                    nameof(request.CategoryId)));
        }
        if (request.BrandId.HasValue)
        {
            var brand = await brandReadRepository.GetByIdAsync(request.BrandId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (brand == null)
            {
                errors.Add(
                    Error.BadRequest($"Thương hiệu với ID {request.BrandId} không tồn tại.", nameof(request.BrandId)));
            }
        }
        if (request.Variants?.Count > 0)
        {
            var slugs = request.Variants
                .Select(v => v.UrlSlug?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var slug in slugs)
            {
                var existing = await productVariantReadRepository.GetBySlugAsync(slug!, cancellationToken)
                    .ConfigureAwait(false);
                if (existing != null)
                {
                    errors.Add(
                        Error.BadRequest(
                            $"Đường dẫn (slug) '{slug}' đã được sử dụng bởi sản phẩm khác.",
                            "Variants.UrlSlug"));
                }
            }
        }
        if (errors.Count > 0)
            return Result<ProductDetailForManagerResponse?>.Failure(errors);
        var optionIdToValueMap = new Dictionary<int, Dictionary<string, int>>();
        var optionNameMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (request.Variants?.Count > 0)
        {
            var allOptionValues = new Dictionary<int, HashSet<string>>();
            var potentialOptionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var variantReq in request.Variants)
            {
                if (!string.IsNullOrWhiteSpace(variantReq.VariantName))
                    potentialOptionNames.Add("Phiên bản");
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        if (!int.TryParse(kvp.Key, out _))
                        {
                            var keyName = kvp.Key?.Trim();
                            if (!string.IsNullOrWhiteSpace(keyName))
                                potentialOptionNames.Add(keyName);
                        }
                    }
                }
            }
            if (potentialOptionNames.Count > 0)
            {
                var predefinedOptionsDict = await predefinedOptionReadRepository.GetAllAsDictionaryAsync(
                    cancellationToken)
                    .ConfigureAwait(false);
                var predefinedValues = predefinedOptionsDict.Values.Select(v => v.ToLower()).ToHashSet();
                var predefinedKeys = predefinedOptionsDict.Keys.Select(k => k.ToLower()).ToHashSet();
                var invalidOptions = potentialOptionNames
                    .Where(n => !predefinedValues.Contains(n.ToLower()) && !predefinedKeys.Contains(n.ToLower()))
                    .ToList();
                if (invalidOptions.Count > 0)
                {
                    foreach (var opt in invalidOptions)
                    {
                        errors.Add(Error.BadRequest($"Thuộc tính '{opt}' không hợp lệ.", "Variants.OptionValues"));
                    }
                    return Result<ProductDetailForManagerResponse?>.Failure(errors);
                }
                var searchNames = new HashSet<string>(potentialOptionNames, StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in predefinedOptionsDict)
                {
                    if (potentialOptionNames.Contains(kvp.Value))
                        searchNames.Add(kvp.Key);
                }
                var existingOptions = await optionReadRepository.GetByNamesAsync(
                    searchNames,
                    cancellationToken,
                    DataFetchMode.All)
                    .ConfigureAwait(false);
                if (existingOptions != null)
                {
                    foreach (var opt in existingOptions)
                    {
                        if (opt.Name != null)
                            optionNameMap[opt.Name] = opt.Id;
                    }
                }
                var missingNames = potentialOptionNames.Where(n => !optionNameMap.ContainsKey(n)).ToList();
                if (missingNames.Count > 0)
                {
                    var newOptions = new List<OptionEntity>();
                    foreach (var name in missingNames)
                    {
                        var key = predefinedOptionsDict.FirstOrDefault(
                                kvp => kvp.Value.Equals(name, StringComparison.OrdinalIgnoreCase))
                                .Key ??
                            name;
                        newOptions.Add(new OptionEntity { Name = key });
                    }
                    productVariantInsertRepository.AddOptionRange(newOptions);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    foreach (var opt in newOptions)
                    {
                        if (opt.Name != null)
                        {
                            var originalName = potentialOptionNames.FirstOrDefault(
                                n => n.Equals(opt.Name, StringComparison.OrdinalIgnoreCase) ||
                                    (predefinedOptionsDict.TryGetValue(opt.Name, out var val) &&
                                        val.Equals(n, StringComparison.OrdinalIgnoreCase)));
                            if (originalName != null)
                                optionNameMap[originalName] = opt.Id;
                            optionNameMap[opt.Name] = opt.Id;
                        }
                    }
                }
            }
            foreach (var variantReq in request.Variants)
            {
                if (!string.IsNullOrWhiteSpace(variantReq.VariantName) &&
                    (optionNameMap.TryGetValue("Phiên bản", out var versionOptId) ||
                        optionNameMap.TryGetValue("Version", out versionOptId)))
                {
                    if (!allOptionValues.TryGetValue(versionOptId, out var vSet))
                    {
                        vSet = [];
                        allOptionValues[versionOptId] = vSet;
                    }
                    vSet.Add(variantReq.VariantName.Trim());
                }
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        int optionId = 0;
                        if (int.TryParse(kvp.Key, out var parsedId))
                            optionId = parsedId;
                        else
                        {
                            var keyName = kvp.Key?.Trim();
                            if (!string.IsNullOrWhiteSpace(keyName) &&
                                optionNameMap.TryGetValue(keyName, out var mappedId))
                                optionId = mappedId;
                        }
                        if (optionId > 0)
                        {
                            var valueName = kvp.Value?.Trim();
                            if (!string.IsNullOrWhiteSpace(valueName))
                            {
                                if (!allOptionValues.TryGetValue(optionId, out var value))
                                {
                                    value = [];
                                    allOptionValues[optionId] = value;
                                }
                                value.Add(valueName);
                            }
                        }
                    }
                }
            }
            foreach (var optionKvp in allOptionValues)
            {
                var optionId = optionKvp.Key;
                var valueNames = optionKvp.Value;
                var option = await optionReadRepository.GetByIdAsync(optionId, cancellationToken).ConfigureAwait(false);
                if (option == null)
                    return Result<ProductDetailForManagerResponse?>.Failure(
                        Error.NotFound($"Thuộc tính với ID {optionId} không tồn tại.", "Variants.OptionValues"));
                var valueMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var valueName in valueNames)
                {
                    var existingValue = await optionValueReadRepository.GetByIdAndNameAsync(
                        optionId,
                        valueName,
                        cancellationToken)
                        .ConfigureAwait(false);
                    if (existingValue != null)
                    {
                        valueMap[valueName] = existingValue.Id;
                    } else
                    {
                        var newValue = new OptionValueEntity { OptionId = optionId, Name = valueName };
                        optionValueInsertRepository.Add(newValue);
                        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        valueMap[valueName] = newValue.Id;
                    }
                }
                optionIdToValueMap[optionId] = valueMap;
            }
        }
        var product = new ProductEntity
        {
            Name = request.Name?.Trim(),
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Description = request.Description?.Trim(),
            Weight = request.Weight,
            Dimensions = request.Dimensions?.Trim(),
            Wheelbase = request.Wheelbase,
            SeatHeight = request.SeatHeight,
            GroundClearance = request.GroundClearance,
            FuelCapacity = request.FuelCapacity,
            TireSize = request.TireSize?.Trim(),
            FrontSuspension = request.FrontSuspension?.Trim(),
            RearSuspension = request.RearSuspension?.Trim(),
            EngineType = request.EngineType?.Trim(),
            MaxPower = request.MaxPower?.Trim(),
            OilCapacity = request.OilCapacity,
            FuelConsumption = request.FuelConsumption?.Trim(),
            TransmissionType = request.TransmissionType?.Trim(),
            StarterSystem = request.StarterSystem?.Trim(),
            MaxTorque = request.MaxTorque?.Trim(),
            Displacement = request.Displacement,
            BoreStroke = request.BoreStroke?.Trim(),
            CompressionRatio = request.CompressionRatio?.Trim(),
            FuelSystem = request.FuelSystem?.Trim(),
            FrameType = request.FrameType?.Trim(),
            FrontTireSize = request.FrontTireSize?.Trim(),
            RearTireSize = request.RearTireSize?.Trim(),
            FrontBrake = request.FrontBrake?.Trim(),
            RearBrake = request.RearBrake?.Trim(),
            BatteryType = request.BatteryType?.Trim(),
            LightingSystem = request.LightingSystem?.Trim(),
            DashboardType = request.DashboardType?.Trim(),
            Material = request.Material?.Trim(),
            Origin = request.Origin?.Trim(),
            WarrantyPeriod = request.WarrantyPeriod?.Trim(),
            Unit = request.Unit?.Trim(),
            StdDot = request.StdDot,
            StdEce = request.StdEce,
            StdSnell = request.StdSnell,
            StdJis = request.StdJis,
            OtherStandards = request.OtherStandards?.Trim(),
            ShortDescription = request.ShortDescription?.Trim(),
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            StatusId = "for-sale",
            ProductVariants = [],
            ProductTechnologies = []
        };
        var variantSupplierPriceTargets = new List<(ProductVariant Variant, List<VariantSupplierPriceRequest> SupplierPrices)>(
            );
        var colorSupplierPriceTargets = new List<(ProductVariant Variant, ProductVariantColor Color, List<VariantSupplierPriceRequest> SupplierPrices)>(
            );
        if (request.Variants?.Count > 0)
        {
            foreach (var variantReq in request.Variants)
            {
                var variantDuplicateError = ValidateSupplierPriceUniqueness(
                    variantReq.SupplierPrices ?? [],
                    variantReq.VariantName ?? variantReq.UrlSlug ?? "biến thể");
                if (variantDuplicateError is not null)
                {
                    return Result<ProductDetailForManagerResponse?>.Failure(variantDuplicateError);
                }
                foreach (var color in variantReq.Colors ?? [])
                {
                    if (color is null)
                        continue;
                    var colorLabel = $"{variantReq.VariantName ?? variantReq.UrlSlug ?? "biến thể"} / {color.ColorName ?? "màu"}";
                    var colorDuplicateError = ValidateSupplierPriceUniqueness(color.SupplierPrices ?? [], colorLabel);
                    if (colorDuplicateError is not null)
                    {
                        return Result<ProductDetailForManagerResponse?>.Failure(colorDuplicateError);
                    }
                }
            }
            foreach (var variantReq in request.Variants)
            {
                var variant = new ProductVariant
                {
                    UrlSlug = SlugHelper.GenerateSlug(variantReq.UrlSlug),
                    Price = variantReq.Price,
                    CoverImageUrl = HasColorRequests(variantReq) ? null : variantReq.CoverImageUrl?.Trim(),
                    VariantName = variantReq.VariantName?.Trim(),
                    SKU = variantReq.SKU?.Trim(),
                    Weight = variantReq.Weight,
                    Dimensions = variantReq.Dimensions?.Trim(),
                    Wheelbase = variantReq.Wheelbase,
                    SeatHeight = variantReq.SeatHeight,
                    GroundClearance = variantReq.GroundClearance,
                    FuelCapacity = variantReq.FuelCapacity,
                    TireSize = variantReq.TireSize?.Trim(),
                    FrontBrake = variantReq.FrontBrake?.Trim(),
                    RearBrake = variantReq.RearBrake?.Trim(),
                    FrontSuspension = variantReq.FrontSuspension?.Trim(),
                    RearSuspension = variantReq.RearSuspension?.Trim(),
                    EngineType = variantReq.EngineType?.Trim(),
                    ProductCollectionPhotos = [],
                    ProductVariantColors = [],
                    VariantOptionValues = []
                };
                foreach (var color in variantReq.Colors)
                {
                    var colorEntity = new ProductVariantColor
                    {
                        ColorName = color.ColorName?.Trim(),
                        ColorCode = color.ColorCode?.Trim(),
                        CoverImageUrl = color.CoverImageUrl?.Trim()
                    };
                    variant.ProductVariantColors.Add(colorEntity);
                    colorSupplierPriceTargets.Add((variant, colorEntity, color.SupplierPrices ?? []));
                }
                if (variantReq.PhotoCollection?.Count > 0)
                {
                    foreach (var photoUrl in variantReq.PhotoCollection.Where(p => !string.IsNullOrWhiteSpace(p)))
                        variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                }
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        int? resolvedOptionId = null;
                        if (int.TryParse(kvp.Key, out var optionId))
                        {
                            resolvedOptionId = optionId;
                        } else
                        {
                            var keyName = kvp.Key?.Trim();
                            if (!string.IsNullOrWhiteSpace(keyName) &&
                                optionNameMap.TryGetValue(keyName, out var mappedId))
                            {
                                resolvedOptionId = mappedId;
                            }
                        }
                        if (resolvedOptionId.HasValue)
                        {
                            var valueName = kvp.Value?.Trim();
                            if (!string.IsNullOrWhiteSpace(valueName) &&
                                optionIdToValueMap.TryGetValue(
                                    resolvedOptionId.Value,
                                    out Dictionary<string, int>? value) &&
                                value.TryGetValue(valueName, out var valueId))
                            {
                                variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = valueId });
                            }
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(variant.VariantName))
                {
                    if (optionNameMap.TryGetValue("Phiên bản", out var versionOptionId) ||
                        optionNameMap.TryGetValue("Version", out versionOptionId))
                    {
                        if (optionIdToValueMap.TryGetValue(versionOptionId, out var versionValueMap) &&
                            versionValueMap.TryGetValue(variant.VariantName, out var versionValueId))
                        {
                            if (!variant.VariantOptionValues.Any(vov => vov.OptionValueId == versionValueId))
                            {
                                variant.VariantOptionValues
                                    .Add(new VariantOptionValue { OptionValueId = versionValueId });
                            }
                        }
                    }
                }
                variant.Weight = variantReq.Weight;
                variant.Dimensions = variantReq.Dimensions?.Trim();
                variant.Wheelbase = variantReq.Wheelbase;
                variant.SeatHeight = variantReq.SeatHeight;
                variant.GroundClearance = variantReq.GroundClearance;
                variant.FuelCapacity = variantReq.FuelCapacity;
                variant.TireSize = variantReq.TireSize?.Trim();
                variant.FrontBrake = variantReq.FrontBrake?.Trim();
                variant.RearBrake = variantReq.RearBrake?.Trim();
                variant.FrontSuspension = variantReq.FrontSuspension?.Trim();
                variant.RearSuspension = variantReq.RearSuspension?.Trim();
                variant.EngineType = variantReq.EngineType?.Trim();
                product.ProductVariants.Add(variant);
                variantSupplierPriceTargets.Add((variant, variantReq.SupplierPrices ?? []));
            }
        }
        if (request.CompatibleVehicleModelIds?.Count > 0)
        {
            foreach (var vehicleId in request.CompatibleVehicleModelIds.Distinct())
            {
                product.CompatibleWith.Add(new ProductCompatibility { CompatibleVehicleModelId = vehicleId });
            }
        }
        if (request.ProductTechnologies?.Count > 0)
        {
            var techList = request.ProductTechnologies.GroupBy(x => x.TechnologyId).Select(g => g.First()).ToList();
            var techIds = techList.Select(x => x.TechnologyId).ToList();
            var validTechs = await technologyReadRepository.GetByIdsAsync(techIds, cancellationToken)
                .ConfigureAwait(false);
            var validTechIds = validTechs.Select(t => t.Id).ToHashSet();
            var invalidIds = techIds.Where(id => !validTechIds.Contains(id)).ToList();
            if (invalidIds.Count > 0)
            {
                return Result<ProductDetailForManagerResponse?>.Failure(
                    Error.BadRequest(
                        $"Các công nghệ sau không tồn tại: {string.Join(", ", invalidIds)}.",
                        nameof(request.ProductTechnologies)));
            }
            var order = 1;
            foreach (var t in techList)
            {
                product.ProductTechnologies
                    .Add(
                        new ProductTechnology
                        {
                            TechnologyId = t.TechnologyId,
                            CustomTitle = t.CustomTitle,
                            CustomDescription = t.CustomDescription,
                            CustomImageUrl = t.CustomImageUrl,
                            DisplayOrder = order++
                        });
            }
        }
        productInsertRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await SyncVariantSupplierPricesAsync(variantSupplierPriceTargets, colorSupplierPriceTargets, cancellationToken)
            .ConfigureAwait(false);
        var response = product.Adapt<ProductDetailForManagerResponse>();
        response?.CompatibleVehicleModelIds = [.. product.CompatibleWith.Select(c => c.CompatibleVehicleModelId)];
        await PopulateSupplierPricesAsync(response, product, cancellationToken).ConfigureAwait(false);
        return Result<ProductDetailForManagerResponse?>.Success(response);
    }

    private async Task SyncVariantSupplierPricesAsync(
        List<(ProductVariant Variant, List<VariantSupplierPriceRequest> SupplierPrices)> variantSupplierPriceTargets,
        List<(ProductVariant Variant, ProductVariantColor Color, List<VariantSupplierPriceRequest> SupplierPrices)> colorSupplierPriceTargets,
        CancellationToken cancellationToken)
    {
        if (ProductQuotationReadRepository is null ||
            ProductQuotationInsertRepository is null ||
            ProductQuotationUpdateRepository is null ||
            ProductQuotationDeleteRepository is null)
        {
            return;
        }
        foreach (var (variant, supplierPrices) in variantSupplierPriceTargets)
        {
            if (variant.Id <= 0)
            {
                continue;
            }
            var existingRows = await ProductQuotationReadRepository.GetByVariantAsync(variant.Id, cancellationToken)
                .ConfigureAwait(false);
            existingRows = [.. existingRows.Where(x => x.ProductVariantColorId == null)];
            var desiredKeys = supplierPrices
                .Select(x => (x.SupplierId, x.ProductVariantColorId))
                .ToHashSet();
            var existingRowsByKey = existingRows.ToDictionary(x => (x.SupplierId ?? 0, x.ProductVariantColorId));
            foreach (var supplierPrice in supplierPrices)
            {
                var key = (supplierPrice.SupplierId, supplierPrice.ProductVariantColorId);
                if (existingRowsByKey.TryGetValue(key, out var existingRow))
                {
                    existingRow.QuotePrice = supplierPrice.QuotePrice.HasValue
                        ? Convert.ToInt32(supplierPrice.QuotePrice.Value)
                        : null;
                    existingRow.Note = supplierPrice.Note?.Trim();
                    ProductQuotationUpdateRepository.Update(existingRow);
                } else
                {
                    await ProductQuotationInsertRepository.AddAsync(
                        new ProductQuotation
                        {
                            ProductVariantId = variant.Id,
                            ProductVariantColorId = supplierPrice.ProductVariantColorId,
                            SupplierId = supplierPrice.SupplierId,
                            QuotePrice =
                                supplierPrice.QuotePrice.HasValue
                                        ? Convert.ToInt32(supplierPrice.QuotePrice.Value)
                                        : null,
                            Note = supplierPrice.Note?.Trim()
                        },
                        cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            foreach (var existingRow in existingRows)
            {
                var key = (existingRow.SupplierId ?? 0, existingRow.ProductVariantColorId);
                if (!desiredKeys.Contains(key))
                {
                    ProductQuotationDeleteRepository.Delete(existingRow);
                }
            }
        }
        foreach (var (variant, color, supplierPrices) in colorSupplierPriceTargets)
        {
            if (color == null || variant.Id <= 0 || color.Id <= 0)
            {
                continue;
            }
            var existingRows = await ProductQuotationReadRepository.GetByVariantAsync(variant.Id, cancellationToken)
                .ConfigureAwait(false);
            existingRows = [.. existingRows.Where(x => x.ProductVariantColorId == color.Id)];
            var desiredKeys = supplierPrices.Select(x => (x.SupplierId, x.ProductVariantColorId ?? color.Id))
                .ToHashSet();
            var existingRowsByKey = existingRows.ToDictionary(x => (x.SupplierId ?? 0, x.ProductVariantColorId));
            foreach (var supplierPrice in supplierPrices)
            {
                var key = (supplierPrice.SupplierId, supplierPrice.ProductVariantColorId ?? color.Id);
                if (existingRowsByKey.TryGetValue(key, out var existingRow))
                {
                    existingRow.QuotePrice = supplierPrice.QuotePrice.HasValue
                        ? Convert.ToInt32(supplierPrice.QuotePrice.Value)
                        : null;
                    existingRow.Note = supplierPrice.Note?.Trim();
                    ProductQuotationUpdateRepository.Update(existingRow);
                } else
                {
                    await ProductQuotationInsertRepository.AddAsync(
                        new ProductQuotation
                        {
                            ProductVariantId = variant.Id,
                            ProductVariantColorId = color.Id,
                            SupplierId = supplierPrice.SupplierId,
                            QuotePrice =
                                supplierPrice.QuotePrice.HasValue
                                        ? Convert.ToInt32(supplierPrice.QuotePrice.Value)
                                        : null,
                            Note = supplierPrice.Note?.Trim()
                        },
                        cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            foreach (var existingRow in existingRows)
            {
                var key = (existingRow.SupplierId ?? 0, existingRow.ProductVariantColorId ?? color.Id);
                if (!desiredKeys.Contains(key))
                {
                    ProductQuotationDeleteRepository.Delete(existingRow);
                }
            }
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task PopulateSupplierPricesAsync(
        ProductDetailForManagerResponse? response,
        ProductEntity product,
        CancellationToken cancellationToken)
    {
        if (ProductQuotationReadRepository is null ||
            ProductQuotationInsertRepository is null ||
            ProductQuotationUpdateRepository is null ||
            ProductQuotationDeleteRepository is null)
        {
            return;
        }
        if (response?.Variants is null || response.Variants.Count == 0)
        {
            return;
        }
        foreach (var responseVariant in response.Variants)
        {
            var variantEntity = product.ProductVariants.FirstOrDefault(v => v.Id == responseVariant.Id);
            if (variantEntity is null || variantEntity.Id <= 0)
            {
                continue;
            }
            var rows = await ProductQuotationReadRepository.GetByVariantAsync(variantEntity.Id, cancellationToken)
                .ConfigureAwait(false);
            responseVariant.SupplierPrices = [.. rows
                .Where(row => row.ProductVariantColorId == null)
                .Select(
                    row => new VariantSupplierPriceRequest
                    {
                        SupplierId = row.SupplierId ?? 0,
                        ProductVariantColorId = row.ProductVariantColorId,
                        QuotePrice = row.QuotePrice,
                        Note = row.Note
                    })];
            var responseColors = responseVariant.Colors ?? [];
            foreach (var responseColor in responseColors)
            {
                var colorId = responseColor.Id;
                if (colorId <= 0)
                {
                    continue;
                }
                responseColor.SupplierPrices = [.. rows
                    .Where(row => row.ProductVariantColorId == colorId)
                    .Select(
                        row => new VariantSupplierPriceRequest
                        {
                            SupplierId = row.SupplierId ?? 0,
                            ProductVariantColorId = row.ProductVariantColorId,
                            QuotePrice = row.QuotePrice,
                            Note = row.Note
                        })];
            }
        }
    }

    private static bool HasColorRequests(CreateProductVariantRequest variant)
    {
        return variant.Colors.Count > 0;
    }

    private static Error? ValidateSupplierPriceUniqueness(
        IEnumerable<VariantSupplierPriceRequest> supplierPrices,
        string scopeLabel)
    {
        var seen = new HashSet<int>();
        foreach (var supplierPrice in supplierPrices)
        {
            if (supplierPrice.SupplierId <= 0)
            {
                continue;
            }
            if (!seen.Add(supplierPrice.SupplierId))
            {
                return Error.BadRequest(
                    $"Mỗi nhà cung cấp chỉ được xuất hiện một lần trong {scopeLabel}.",
                    nameof(VariantSupplierPriceRequest.SupplierId));
            }
        }
        return null;
    }
}