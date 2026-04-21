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
using Application.Interfaces.Repositories.VariantOptionValue;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductReadRepository productReadRepository,
    IBrandReadRepository brandReadRepository,
    IProductCategoryReadRepository productCategoryReadRepository,
    IProductVariantReadRepository productVariantReadRepository,
    IOptionReadRepository optionReadRepository,
    IPredefinedOptionReadRepository predefinedOptionReadRepository,
    IOptionValueReadRepository optionValueReadRepository,
    IOptionValueInsertRepository optionValueInsertRepository,
    IProductVariantInsertRepository productVariantInsertRepository,
    IVariantOptionValueDeleteRepository variantOptionValueDeleteRepository,
    IProductUpdateRepository updateRepository,
    IProductVarientDeleteRepository productVarientDeleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        var product = await productReadRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken)
            .ConfigureAwait(false);

        if(product is null)
        {
            return Error.NotFound($"Không tìm thấy sản phẩm với ID {command.Id}.");
        }

        if(command.CategoryId.HasValue)
        {
            var category = await productCategoryReadRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken)
                .ConfigureAwait(false);
            if(category is null)
            {
                errors.Add(
                    Error.NotFound(
                        $"Danh mục sản phẩm với ID {command.CategoryId} không tồn tại hoặc đã bị xóa.",
                        nameof(command.CategoryId)));
            }
        }

        if(command.BrandId.HasValue)
        {
            var brand = await brandReadRepository.GetByIdAsync(command.BrandId.Value, cancellationToken)
                .ConfigureAwait(false);
            if(brand is null)
            {
                errors.Add(
                    Error.NotFound(
                        $"Thương hiệu với ID {command.BrandId} không tồn tại hoặc đã bị xóa.",
                        nameof(command.BrandId)));
            }
        }

        if(command.Variants?.Count > 0)
        {
            var slugs = command.Variants
                .Select(v => v.UrlSlug?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if(slugs.Count != slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                errors.Add(Error.BadRequest("Tìm thấy các đường dẫn (slug) trùng lặp trong yêu cầu.", nameof(command.Variants)));
            }

            foreach(var variantReq in command.Variants.Where(v => !string.IsNullOrWhiteSpace(v.UrlSlug)))
            {
                var existing = await productVariantReadRepository.GetBySlugAsync(
                    variantReq.UrlSlug!.Trim(),
                    cancellationToken)
                    .ConfigureAwait(false);
                if(existing is not null)
                {
                    if(existing.ProductId == command.Id && existing.Id == variantReq.Id)
                    {
                        continue;
                    }

                    errors.Add(Error.BadRequest($"Đường dẫn (slug) '{variantReq.UrlSlug}' đã được sử dụng bởi sản phẩm khác.", "Variants.UrlSlug"));
                }
            }
        }

        if(errors.Count > 0)
        {
            return errors;
        }

        product.Name = command.Name?.Trim();
        product.CategoryId = command.CategoryId;
        product.BrandId = command.BrandId;
        product.Description = command.Description?.Trim();
        product.Weight = command.Weight;
        product.Dimensions = command.Dimensions?.Trim();
        product.Wheelbase = command.Wheelbase;
        product.SeatHeight = command.SeatHeight;
        product.GroundClearance = command.GroundClearance;
        product.FuelCapacity = command.FuelCapacity;
        product.TireSize = command.TireSize?.Trim();
        product.FrontSuspension = command.FrontSuspension?.Trim();
        product.RearSuspension = command.RearSuspension?.Trim();
        product.EngineType = command.EngineType?.Trim();
        product.MaxPower = command.MaxPower?.Trim();
        product.OilCapacity = command.OilCapacity;
        product.FuelConsumption = command.FuelConsumption?.Trim();
        product.TransmissionType = command.TransmissionType?.Trim();
        product.StarterSystem = command.StarterSystem?.Trim();
        product.MaxTorque = command.MaxTorque?.Trim();
        product.Displacement = command.Displacement;
        product.BoreStroke = command.BoreStroke?.Trim();
        product.CompressionRatio = command.CompressionRatio?.Trim();
        product.ShortDescription = command.ShortDescription?.Trim();
        product.MetaTitle = command.MetaTitle?.Trim();
        product.MetaDescription = command.MetaDescription?.Trim();

        var optionIdToValueMap = new Dictionary<int, Dictionary<string, int>>();
        var optionNameMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var inputVariants = command.Variants ?? [];

        if(inputVariants.Count > 0)
        {
            var allOptionValues = new Dictionary<int, HashSet<string>>();
            var potentialOptionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach(var variantReq in inputVariants)
            {
                // Add specialized fields to potential names for sync
                if (!string.IsNullOrWhiteSpace(variantReq.ColorName)) potentialOptionNames.Add("Màu sắc");
                if (!string.IsNullOrWhiteSpace(variantReq.VersionName)) potentialOptionNames.Add("Phiên bản");

                if(variantReq.OptionValues?.Count > 0)
                {
                    foreach(var kvp in variantReq.OptionValues)
                    {
                        if(!int.TryParse(kvp.Key, out _))
                        {
                            var keyName = kvp.Key?.Trim();
                            if(!string.IsNullOrWhiteSpace(keyName))
                            {
                                potentialOptionNames.Add(keyName);
                            }
                        }
                    }
                }
            }

            if(potentialOptionNames.Count > 0)
            {
                var predefinedOptionsDict = await predefinedOptionReadRepository.GetAllAsDictionaryAsync(cancellationToken)
                    .ConfigureAwait(false);
                var allowedNamesSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in predefinedOptionsDict)
                {
                    allowedNamesSet.Add(kvp.Key);
                    allowedNamesSet.Add(kvp.Value);
                }

                var invalidOptions = potentialOptionNames.Where(n => !allowedNamesSet.Contains(n)).ToList();
                if(invalidOptions.Count > 0)
                {
                    return Result<ProductDetailForManagerResponse?>.Failure(
                        Error.BadRequest(
                            $"Các tên thuộc tính sau không hợp lệ: {string.Join(", ", invalidOptions)}. Vui lòng chỉ sử dụng các thuộc tính đã được định nghĩa sẵn.",
                            "Variants.OptionValues"));
                }

                var existingOptions = await optionReadRepository.GetByNamesAsync(
                    potentialOptionNames,
                    cancellationToken,
                    DataFetchMode.All)
                    .ConfigureAwait(false);

                foreach(var opt in existingOptions)
                {
                    if(opt.Name is not null)
                    {
                        optionNameMap[opt.Name] = opt.Id;
                    }
                }

                var missingNames = potentialOptionNames.Where(n => !optionNameMap.ContainsKey(n)).ToList();
                if(missingNames.Count > 0)
                {
                    var newOptions = new List<OptionEntity>();
                    foreach (var name in missingNames)
                    {
                        // Map back to Key if it's a Value
                        var key = predefinedOptionsDict.FirstOrDefault(kvp => 
                            kvp.Value.Equals(name, StringComparison.OrdinalIgnoreCase)).Key ?? name;
                        
                        newOptions.Add(new OptionEntity { Name = key });
                    }
                    
                    productVariantInsertRepository.AddOptionRange(newOptions);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    foreach(var opt in newOptions)
                    {
                        if(opt.Name is not null)
                        {
                            // Map the original name (could be label or key) to the created ID
                            var originalName = potentialOptionNames.FirstOrDefault(n => 
                                n.Equals(opt.Name, StringComparison.OrdinalIgnoreCase) || 
                                (predefinedOptionsDict.TryGetValue(opt.Name, out var val) && val.Equals(n, StringComparison.OrdinalIgnoreCase)));
                            
                            if (originalName != null)
                                optionNameMap[originalName] = opt.Id;
                            
                            // Also ensure the key itself is in the map
                            optionNameMap[opt.Name] = opt.Id;
                        }
                    }
                }
            }

            foreach(var variantReq in inputVariants)
            {
                // Collect values from specialized fields
                if (!string.IsNullOrWhiteSpace(variantReq.ColorName) && optionNameMap.TryGetValue("Màu sắc", out var colorOptId))
                {
                    if (!allOptionValues.TryGetValue(colorOptId, out var vSet))
                    {
                        vSet = [];
                        allOptionValues[colorOptId] = vSet;
                    }
                    // Split multi-color strings
                    var names = variantReq.ColorName.Split(',').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n));
                    foreach (var name in names) vSet.Add(name);
                }
                if (!string.IsNullOrWhiteSpace(variantReq.VersionName) && optionNameMap.TryGetValue("Phiên bản", out var versionOptId))
                {
                    if (!allOptionValues.TryGetValue(versionOptId, out var vSet))
                    {
                        vSet = [];
                        allOptionValues[versionOptId] = vSet;
                    }
                    vSet.Add(variantReq.VersionName.Trim());
                }

                if(variantReq.OptionValues?.Count > 0)
                {
                    foreach(var kvp in variantReq.OptionValues)
                    {
                        var optionId = 0;
                        if(int.TryParse(kvp.Key, out var parsedId))
                        {
                            optionId = parsedId;
                        } else
                        {
                            var keyName = kvp.Key?.Trim();
                            if(!string.IsNullOrWhiteSpace(keyName) &&
                                optionNameMap.TryGetValue(keyName, out var mappedId))
                            {
                                optionId = mappedId;
                            }
                        }

                        if(optionId > 0)
                        {
                            var valueName = kvp.Value?.Trim();
                            if(!string.IsNullOrWhiteSpace(valueName))
                            {
                                if(!allOptionValues.TryGetValue(optionId, out var valueSet))
                                {
                                    valueSet = [];
                                    allOptionValues[optionId] = valueSet;
                                }
                                valueSet.Add(valueName);
                            }
                        }
                    }
                }
            }

            var colorNamesWithCodes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (optionNameMap.TryGetValue("Màu sắc", out var colorOptionIdKey) || optionNameMap.TryGetValue("Color", out colorOptionIdKey))
            {
                foreach (var vReq in inputVariants)
                {
                    if (!string.IsNullOrWhiteSpace(vReq.ColorName))
                    {
                        var names = vReq.ColorName.Split(',').Select(n => n.Trim()).ToList();
                        var codes = (vReq.ColorCode ?? "").Split(',').Select(c => c.Trim()).ToList();
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

            foreach(var optionKvp in allOptionValues)
            {
                var optionId = optionKvp.Key;
                var valueNames = optionKvp.Value;

                var option = await optionReadRepository.GetByIdAsync(optionId, cancellationToken).ConfigureAwait(false);
                if(option is null)
                {
                    return Result<ProductDetailForManagerResponse?>.Failure(
                        Error.NotFound($"Thuộc tính với ID {optionId} không tồn tại.", "Variants.OptionValues"));
                }

                var valueMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach(var valueName in valueNames)
                {
                    var existingValue = await optionValueReadRepository.GetByIdAndNameAsync(
                        optionId,
                        valueName,
                        cancellationToken)
                        .ConfigureAwait(false);

                    if(existingValue is not null)
                    {
                        valueMap[valueName] = existingValue.Id;
                        // Update color code if it's missing but we have it now
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

        var currentVariants = product.ProductVariants.ToList();
        var inputVariantIds = inputVariants.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet();
        var variantsToDelete = currentVariants.Where(v => !inputVariantIds.Contains(v.Id)).ToList();

        foreach(var v in variantsToDelete)
        {
            productVarientDeleteRepository.Delete(v);
            product.ProductVariants.Remove(v);
        }

        foreach(var variantReq in inputVariants)
        {
            ProductVariant variantEntity;

            if(variantReq.Id.HasValue && variantReq.Id > 0)
            {
                variantEntity = currentVariants.FirstOrDefault(v => v.Id == variantReq.Id.Value)!;
                if(variantEntity is null)
                {
                    continue;
                }
            } else
            {
                variantEntity = new ProductVariant { ProductId = command.Id };
                product.ProductVariants.Add(variantEntity);
            }

            variantEntity.UrlSlug = SlugHelper.GenerateSlug(variantReq.UrlSlug);
            variantEntity.Price = variantReq.Price;
            variantEntity.CoverImageUrl = variantReq.CoverImageUrl?.Trim();
            variantEntity.VersionName = variantReq.VersionName?.Trim();
            variantEntity.ColorName = variantReq.ColorName?.Trim();
            variantEntity.ColorCode = variantReq.ColorCode?.Trim();
            variantEntity.SKU = variantReq.SKU?.Trim();

            UpdateVariantPhotos(variantEntity, variantReq.PhotoCollection);

            var currentLinks = variantEntity.VariantOptionValues.ToList();
            foreach(var link in currentLinks)
            {
                variantOptionValueDeleteRepository.Delete(link);
                variantEntity.VariantOptionValues.Remove(link);
            }

            if(variantReq.OptionValues?.Count > 0)
            {
                foreach(var kvp in variantReq.OptionValues)
                {
                    int? resolvedOptionId = null;
                    if(int.TryParse(kvp.Key, out var optionId))
                    {
                        resolvedOptionId = optionId;
                    } else
                    {
                        var keyName = kvp.Key?.Trim();
                        if(!string.IsNullOrWhiteSpace(keyName) && optionNameMap.TryGetValue(keyName, out var mappedId))
                        {
                            resolvedOptionId = mappedId;
                        }
                    }

                    if(resolvedOptionId.HasValue)
                    {
                        var valueName = kvp.Value?.Trim();
                        if(!string.IsNullOrWhiteSpace(valueName) &&
                            optionIdToValueMap.TryGetValue(resolvedOptionId.Value, out var valueMap) &&
                            valueMap.TryGetValue(valueName, out var valueId))
                        {
                            variantEntity.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = valueId });
                        }
                    }
                }
            }

            // --- AUTO-SYNC SPECIALIZED FIELDS TO DYNAMIC ATTRIBUTES ---
            // Sync Color
            if (!string.IsNullOrWhiteSpace(variantEntity.ColorName))
            {
                if (optionNameMap.TryGetValue("Màu sắc", out var colorOptionId) || optionNameMap.TryGetValue("Color", out colorOptionId))
                {
                    if (optionIdToValueMap.TryGetValue(colorOptionId, out var colorValueMap))
                    {
                        var names = variantEntity.ColorName.Split(',').Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n));
                        foreach (var name in names)
                        {
                            if (colorValueMap.TryGetValue(name, out var colorValueId))
                            {
                                if (!variantEntity.VariantOptionValues.Any(vov => vov.OptionValueId == colorValueId))
                                {
                                    variantEntity.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = colorValueId });
                                }
                            }
                        }
                    }
                }
            }
            // Sync Version
            if (!string.IsNullOrWhiteSpace(variantEntity.VersionName))
            {
                if (optionNameMap.TryGetValue("Phiên bản", out var versionOptionId) || optionNameMap.TryGetValue("Version", out versionOptionId))
                {
                    if (optionIdToValueMap.TryGetValue(versionOptionId, out var versionValueMap) && 
                        versionValueMap.TryGetValue(variantEntity.VersionName, out var versionValueId))
                    {
                        if (!variantEntity.VariantOptionValues.Any(vov => vov.OptionValueId == versionValueId))
                        {
                            variantEntity.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = versionValueId });
                        }
                    }
                }
            }
            // ---------------------------------------------------------
        }

        // Sync ProductTechnologies
        var existingTechs = product.ProductTechnologies.ToList();
        var newTechList = new List<TechnologyJsonRequest>();
        if (!string.IsNullOrWhiteSpace(command.Highlights))
        {
            try { newTechList = System.Text.Json.JsonSerializer.Deserialize<List<TechnologyJsonRequest>>(command.Highlights) ?? []; }
            catch { /* ignore */ }
        }

        // Remove old ones
        foreach (var existing in existingTechs)
        {
            if (!newTechList.Any(t => t.TechnologyId == existing.TechnologyId))
            {
                product.ProductTechnologies.Remove(existing);
            }
        }

        // Add or Update
        var order = 1;
        foreach (var t in newTechList)
        {
            var existing = existingTechs.FirstOrDefault(et => et.TechnologyId == t.TechnologyId);
            if (existing != null)
            {
                existing.CustomTitle = t.CustomTitle;
                existing.CustomDescription = t.CustomDescription;
                existing.CustomImageUrl = t.CustomImageUrl;
                existing.DisplayOrder = order++;
            }
            else
            {
                product.ProductTechnologies.Add(new Domain.Entities.ProductTechnology
                {
                    TechnologyId = t.TechnologyId,
                    CustomTitle = t.CustomTitle,
                    CustomDescription = t.CustomDescription,
                    CustomImageUrl = t.CustomImageUrl,
                    DisplayOrder = order++
                });
            }
        }

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = product.Adapt<ProductDetailForManagerResponse>();

        return response;
    }

    private static void UpdateVariantPhotos(ProductVariant variant, List<string>? newUrls)
    {
        var targetParams = (newUrls ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var currentPhotos = variant.ProductCollectionPhotos.ToList();

        foreach(var photo in currentPhotos)
        {
            if(!string.IsNullOrWhiteSpace(photo.ImageUrl) && !targetParams.Contains(photo.ImageUrl.Trim()))
            {
                variant.ProductCollectionPhotos.Remove(photo);
            }
        }

        var existingUrls = currentPhotos
            .Where(p => !string.IsNullOrWhiteSpace(p.ImageUrl))
            .Select(p => p.ImageUrl!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach(var url in targetParams)
        {
            if(!existingUrls.Contains(url))
            {
                variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = url });
            }
        }
    }

    private class TechnologyJsonRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("technologyId")]
        public int TechnologyId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("customTitle")]
        public string? CustomTitle { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("customDescription")]
        public string? CustomDescription { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("customImageUrl")]
        public string? CustomImageUrl { get; set; }
    }
}