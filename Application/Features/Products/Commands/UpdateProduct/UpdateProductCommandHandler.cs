using Application.ApiContracts.Product.Responses;
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
    IProductVariantReadRepository productVariantReadRepository,
    IProductCategoryReadRepository productCategoryReadRepository,
    IBrandReadRepository brandReadRepository,
    IPredefinedOptionReadRepository predefinedOptionReadRepository,
    IOptionReadRepository optionReadRepository,
    IOptionValueReadRepository optionValueReadRepository,
    IProductVariantInsertRepository productVariantInsertRepository,
    IOptionValueInsertRepository optionValueInsertRepository,
    IVariantOptionValueDeleteRepository variantOptionValueDeleteRepository,
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
        if (product is null)
        {
            return Error.NotFound($"Product with Id {command.Id} not found.");
        }
        if (command.CategoryId.HasValue)
        {
            var category = await productCategoryReadRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (category is null)
            {
                errors.Add(
                    Error.NotFound(
                        $"Product category with Id {command.CategoryId} not found or has been deleted.",
                        nameof(command.CategoryId)));
            }
        }
        if (command.BrandId.HasValue)
        {
            var brand = await brandReadRepository.GetByIdAsync(command.BrandId.Value, cancellationToken)
                .ConfigureAwait(false);
            if (brand is null)
            {
                errors.Add(
                    Error.NotFound(
                        $"Brand with Id {command.BrandId} not found or has been deleted.",
                        nameof(command.BrandId)));
            }
        }
        if (command.Variants?.Count > 0)
        {
            var slugs = command.Variants
                .Select(v => v.UrlSlug?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            if (slugs.Count != slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                errors.Add(Error.BadRequest("Duplicate slugs found within the command.", nameof(command.Variants)));
            }
            foreach (var variantReq in command.Variants.Where(v => !string.IsNullOrWhiteSpace(v.UrlSlug)))
            {
                var existing = await productVariantReadRepository.GetBySlugAsync(
                    variantReq.UrlSlug!.Trim(),
                    cancellationToken)
                    .ConfigureAwait(false);
                if (existing is not null)
                {
                    if (existing.ProductId == command.Id && existing.Id == variantReq.Id)
                    {
                        continue;
                    }
                    errors.Add(Error.BadRequest($"Slug '{variantReq.UrlSlug}' is already in use.", "Variants.UrlSlug"));
                }
            }
        }
        if (errors.Count > 0)
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
        if (inputVariants.Count > 0)
        {
            var allOptionValues = new Dictionary<int, HashSet<string>>();
            var potentialOptionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var variantReq in inputVariants)
            {
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
                var allowedKeys = await predefinedOptionReadRepository.GetAllKeysAsync(cancellationToken)
                    .ConfigureAwait(false);
                var invalidOptions = potentialOptionNames.Except(allowedKeys, StringComparer.OrdinalIgnoreCase).ToList();
                if (invalidOptions.Count > 0)
                {
                    return Result<ProductDetailForManagerResponse?>.Failure(
                        Error.BadRequest(
                            $"The following option names are invalid: {string.Join(", ", invalidOptions)}",
                            "Variants.OptionValues"));
                }
                var existingOptions = await optionReadRepository.GetByNamesAsync(
                    potentialOptionNames,
                    cancellationToken,
                    DataFetchMode.All)
                    .ConfigureAwait(false);
                foreach (var opt in existingOptions)
                {
                    if (opt.Name is not null)
                    {
                        optionNameMap[opt.Name] = opt.Id;
                    }
                }
                var missingNames = potentialOptionNames.Where(n => !optionNameMap.ContainsKey(n)).ToList();
                if (missingNames.Count > 0)
                {
                    var newOptions = missingNames.Select(n => new OptionEntity { Name = n }).ToList();
                    productVariantInsertRepository.AddOptionRange(newOptions);
                    await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    foreach (var opt in newOptions)
                    {
                        if (opt.Name is not null)
                        {
                            optionNameMap[opt.Name] = opt.Id;
                        }
                    }
                }
            }
            foreach (var variantReq in inputVariants)
            {
                if (variantReq.OptionValues?.Count > 0)
                {
                    foreach (var kvp in variantReq.OptionValues)
                    {
                        var optionId = 0;
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
                                if (!allOptionValues.TryGetValue(optionId, out var valueSet))
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
            foreach (var optionKvp in allOptionValues)
            {
                var optionId = optionKvp.Key;
                var valueNames = optionKvp.Value;
                var option = await optionReadRepository.GetByIdAsync(optionId, cancellationToken).ConfigureAwait(false);
                if (option is null)
                {
                    return Result<ProductDetailForManagerResponse?>.Failure(
                        Error.NotFound($"Option with Id {optionId} not found.", "Variants.OptionValues"));
                }
                var valueMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var valueName in valueNames)
                {
                    var existingValue = await optionValueReadRepository.GetByIdAndNameAsync(
                        optionId,
                        valueName,
                        cancellationToken)
                        .ConfigureAwait(false);
                    if (existingValue is not null)
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
        var currentVariants = product.ProductVariants.ToList();
        var inputVariantIds = inputVariants.Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet();
        var variantsToDelete = currentVariants.Where(v => !inputVariantIds.Contains(v.Id)).ToList();
        foreach (var v in variantsToDelete)
        {
            productVarientDeleteRepository.Delete(v);
            product.ProductVariants.Remove(v);
        }
        foreach (var variantReq in inputVariants)
        {
            ProductVariant variantEntity;
            if (variantReq.Id.HasValue && variantReq.Id > 0)
            {
                variantEntity = currentVariants.FirstOrDefault(v => v.Id == variantReq.Id.Value)!;
                if (variantEntity is null)
                {
                    continue;
                }
            } else
            {
                variantEntity = new ProductVariant { ProductId = command.Id };
                product.ProductVariants.Add(variantEntity);
            }
            variantEntity.UrlSlug = variantReq.UrlSlug?.Trim();
            variantEntity.Price = variantReq.Price;
            variantEntity.CoverImageUrl = variantReq.CoverImageUrl?.Trim();
            UpdateVariantPhotos(variantEntity, variantReq.PhotoCollection);
            var currentLinks = variantEntity.VariantOptionValues.ToList();
            foreach (var link in currentLinks)
            {
                variantOptionValueDeleteRepository.Delete(link);
            }
            variantEntity.VariantOptionValues.Clear();

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
                        if (!string.IsNullOrWhiteSpace(keyName) && optionNameMap.TryGetValue(keyName, out var mappedId))
                        {
                            resolvedOptionId = mappedId;
                        }
                    }
                    if (resolvedOptionId.HasValue)
                    {
                        var valueName = kvp.Value?.Trim();
                        if (!string.IsNullOrWhiteSpace(valueName) &&
                            optionIdToValueMap.TryGetValue(resolvedOptionId.Value, out var valueMap) &&
                            valueMap.TryGetValue(valueName, out var valueId))
                        {
                            variantEntity.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = valueId });
                        }
                    }
                }
            }
        }
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
        foreach (var photo in currentPhotos)
        {
            if (!string.IsNullOrWhiteSpace(photo.ImageUrl) && !targetParams.Contains(photo.ImageUrl.Trim()))
            {
                variant.ProductCollectionPhotos.Remove(photo);
            }
        }
        var existingUrls = currentPhotos
            .Where(p => !string.IsNullOrWhiteSpace(p.ImageUrl))
            .Select(p => p.ImageUrl!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var url in targetParams)
        {
            if (!existingUrls.Contains(url))
            {
                variant.ProductCollectionPhotos.Add(new ProductCollectionPhoto { ImageUrl = url });
            }
        }
    }
}