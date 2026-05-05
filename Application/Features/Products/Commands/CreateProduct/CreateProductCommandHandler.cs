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
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;

using Domain.Entities;
using Mapster;
using MediatR;
using System.Text.Json;
using System.Text.Json.Serialization;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductCategoryReadRepository productCategoryReadRepository,
    IBrandReadRepository brandReadRepository,
    IProductVariantReadRepository productVariantReadRepository,
    IOptionValueReadRepository optionValueReadRepository,
    IOptionReadRepository optionReadRepository,
    IPredefinedOptionReadRepository predefinedOptionReadRepository,
    IProductInsertRepository productInsertRepository,
    IProductVariantInsertRepository productVariantInsertRepository,
    IOptionValueInsertRepository optionValueInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();
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
        {
            return Result<ProductDetailForManagerResponse?>.Failure(errors);
        }
        var optionIdToValueMap = new Dictionary<int, Dictionary<string, int>>();
        var optionNameMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (request.Variants?.Count > 0)
        {
            var allOptionValues = new Dictionary<int, HashSet<string>>();
            var potentialOptionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var variantReq in request.Variants)
            {
                if (!string.IsNullOrWhiteSpace(variantReq.ColorName))
                    potentialOptionNames.Add("Màu sắc");
                if (!string.IsNullOrWhiteSpace(variantReq.VersionName))
                    potentialOptionNames.Add("Phiên bản");
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        if (!int.TryParse(kvp.Key, out _))
                        {
                            var keyName = kvp.Key?.Trim();
                            if (!string.IsNullOrWhiteSpace(keyName))
                            {
                                potentialOptionNames.Add(keyName);
                            }
                        }
                    }
                }
            }
            if (potentialOptionNames.Count > 0)
            {
                var predefinedOptionsDict = await predefinedOptionReadRepository.GetAllAsDictionaryAsync(
                    cancellationToken)
                    .ConfigureAwait(false);
                var allowedNamesSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in predefinedOptionsDict)
                {
                    allowedNamesSet.Add(kvp.Key);
                    allowedNamesSet.Add(kvp.Value);
                }
                var invalidOptions = potentialOptionNames.Where(n => !allowedNamesSet.Contains(n)).ToList();
                if (invalidOptions.Count > 0)
                {
                    errors.Add(
                        Error.BadRequest(
                            $"Các tên thuộc tính sau không hợp lệ: {string.Join(", ", invalidOptions)}. Vui lòng chỉ sử dụng các thuộc tính đã được định nghĩa sẵn.",
                            "Variants.OptionValues"));
                    return Result<ProductDetailForManagerResponse?>.Failure(errors);
                }
                var existingOptions = await optionReadRepository.GetByNamesAsync(
                    potentialOptionNames,
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
                if (!string.IsNullOrWhiteSpace(variantReq.ColorName) &&
                    optionNameMap.TryGetValue("Màu sắc", out var colorOptId))
                {
                    if (!allOptionValues.TryGetValue(colorOptId, out var vSet))
                    {
                        vSet = [];
                        allOptionValues[colorOptId] = vSet;
                    }
                    vSet.Add(variantReq.ColorName.Trim());
                }
                if (!string.IsNullOrWhiteSpace(variantReq.VersionName) &&
                    optionNameMap.TryGetValue("Phiên bản", out var versionOptId))
                {
                    if (!allOptionValues.TryGetValue(versionOptId, out var vSet))
                    {
                        vSet = [];
                        allOptionValues[versionOptId] = vSet;
                    }
                    vSet.Add(variantReq.VersionName.Trim());
                }
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        int optionId = 0;
                        if (int.TryParse(kvp.Key, out var parsedId))
                        {
                            optionId = parsedId;
                        } else
                        {
                            var keyName = kvp.Key?.Trim();
                            if (!string.IsNullOrWhiteSpace(keyName) &&
                                optionNameMap.TryGetValue(keyName, out var mappedId))
                            {
                                optionId = mappedId;
                            }
                        }
                        if (optionId > 0)
                        {
                            var valueName = kvp.Value?.Trim();
                            if (!string.IsNullOrWhiteSpace(valueName))
                            {
                                if (!allOptionValues.TryGetValue(optionId, out HashSet<string>? value))
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
            var colorNamesWithCodes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (optionNameMap.TryGetValue("Màu sắc", out _) ||
                optionNameMap.TryGetValue("Color", out _))
            {
                foreach (var vReq in request.Variants)
                {
                    if (!string.IsNullOrWhiteSpace(vReq.ColorName))
                    {
                        var names = vReq.ColorName.Split(',').Select(n => n.Trim()).ToList();
                        var codes = (vReq.ColorCode ?? string.Empty).Split(',').Select(c => c.Trim()).ToList();
                        for (int i = 0; i < names.Count; i++)
                        {
                            var name = names[i];
                            var code = i < codes.Count ? codes[i] : null;
                            if (!string.IsNullOrWhiteSpace(name) && !colorNamesWithCodes.ContainsKey(name))
                            {
                                colorNamesWithCodes[name] = code;
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
                {
                    return Result<ProductDetailForManagerResponse?>.Failure(
                        Error.NotFound($"Thuộc tính với ID {optionId} không tồn tại.", "Variants.OptionValues"));
                }
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
                        if (string.IsNullOrWhiteSpace(existingValue.ColorCode) &&
                            colorNamesWithCodes.TryGetValue(valueName, out var newCode) &&
                            !string.IsNullOrWhiteSpace(newCode))
                        {
                            existingValue.ColorCode = newCode;
                        }
                    } else
                    {
                        var newValue = new OptionValueEntity { OptionId = optionId, Name = valueName };
                        if (colorNamesWithCodes.TryGetValue(valueName, out var code))
                        {
                            newValue.ColorCode = code;
                        }
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
            ShortDescription = request.ShortDescription?.Trim(),
            MetaTitle = request.MetaTitle?.Trim(),
            MetaDescription = request.MetaDescription?.Trim(),
            StatusId = "for-sale",
            ProductVariants = [],
            ProductTechnologies = []
        };
        if (request.Variants?.Count > 0)
        {
            foreach (var variantReq in request.Variants)
            {
                var variant = new ProductVariant
                {
                    UrlSlug = SlugHelper.GenerateSlug(variantReq.UrlSlug),
                    Price = variantReq.Price,
                    CoverImageUrl = variantReq.CoverImageUrl?.Trim(),
                    VersionName = variantReq.VersionName?.Trim(),
                    ColorName = variantReq.ColorName?.Trim(),
                    ColorCode = variantReq.ColorCode?.Trim(),
                    SKU = variantReq.SKU?.Trim(),
                    ProductCollectionPhotos = [],
                    VariantOptionValues = []
                };
                if (variantReq.PhotoCollection?.Count > 0)
                {
                    foreach (var photoUrl in variantReq.PhotoCollection.Where(p => !string.IsNullOrWhiteSpace(p)))
                    {
                        variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = photoUrl.Trim() });
                    }
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
                if (!string.IsNullOrWhiteSpace(variant.ColorName))
                {
                    if (optionNameMap.TryGetValue("Màu sắc", out var colorOptionId) ||
                        optionNameMap.TryGetValue("Color", out colorOptionId))
                    {
                        if (optionIdToValueMap.TryGetValue(colorOptionId, out var colorValueMap) &&
                            colorValueMap.TryGetValue(variant.ColorName, out var colorValueId))
                        {
                            if (!variant.VariantOptionValues.Any(vov => vov.OptionValueId == colorValueId))
                            {
                                variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = colorValueId });
                            }
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(variant.VersionName))
                {
                    if (optionNameMap.TryGetValue("Phiên bản", out var versionOptionId) ||
                        optionNameMap.TryGetValue("Version", out versionOptionId))
                    {
                        if (optionIdToValueMap.TryGetValue(versionOptionId, out var versionValueMap) &&
                            versionValueMap.TryGetValue(variant.VersionName, out var versionValueId))
                        {
                            if (!variant.VariantOptionValues.Any(vov => vov.OptionValueId == versionValueId))
                            {
                                variant.VariantOptionValues
                                    .Add(new VariantOptionValue { OptionValueId = versionValueId });
                            }
                        }
                    }
                }
                product.ProductVariants.Add(variant);
            }
        }
        if (!string.IsNullOrWhiteSpace(request.Highlights))
        {
            try
            {
                var techs = JsonSerializer.Deserialize<List<TechnologyJsonRequest>>(request.Highlights);
                if (techs != null)
                {
                    var order = 1;
                    foreach (var t in techs)
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
            } catch (Exception)
            {
            }
        }
        productInsertRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<ProductDetailForManagerResponse?>.Success(product.Adapt<ProductDetailForManagerResponse>());
    }

    private class TechnologyJsonRequest
    {
        [JsonPropertyName("technologyId")]
        public int TechnologyId { get; set; }

        [JsonPropertyName("customTitle")]
        public string? CustomTitle { get; set; }

        [JsonPropertyName("customDescription")]
        public string? CustomDescription { get; set; }

        [JsonPropertyName("customImageUrl")]
        public string? CustomImageUrl { get; set; }
    }
}