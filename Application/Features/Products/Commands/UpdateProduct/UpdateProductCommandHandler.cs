using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.OptionValue;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductVariant;

using Domain.Entities;
using Mapster;
using MediatR;
using System.Linq;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductReadRepository productReadRepository,
    IBrandReadRepository brandReadRepository,
    IProductCategoryReadRepository productCategoryReadRepository,
    IProductVariantReadRepository productVariantReadRepository,
    IOptionValueReadRepository optionValueReadRepository,
    IOptionValueInsertRepository optionValueInsertRepository,
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

        if(product == null)
        {
            return Error.NotFound($"Product with Id {command.Id} not found.");
        }

        if(command.CategoryId.HasValue)
        {
            var category = await productCategoryReadRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken)
                .ConfigureAwait(false);
            if(category == null)
            {
                errors.Add(Error.NotFound($"Product category with Id {command.CategoryId} not found or has been deleted.", nameof(command.CategoryId)));
            }
        }

        if(command.BrandId.HasValue)
        {
            var brand = await brandReadRepository.GetByIdAsync(command.BrandId.Value, cancellationToken)
                .ConfigureAwait(false);
            if(brand == null)
            {
                errors.Add(Error.NotFound($"Brand with Id {command.BrandId} not found or has been deleted.", nameof(command.BrandId)));
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
                errors.Add(Error.BadRequest("Duplicate slugs found within the command.", "Varients"));
            }

            foreach(var variantReq in command.Variants.Where(v => !string.IsNullOrWhiteSpace(v.UrlSlug)))
            {
                var existing = await productVariantReadRepository.GetBySlugAsync(
                    variantReq.UrlSlug!.Trim(),
                    cancellationToken)
                    .ConfigureAwait(false);
                if(existing != null)
                {
                    if(existing.ProductId == command.Id && existing.Id == variantReq.Id)
                    {
                        continue;
                    }

                    errors.Add(Error.BadRequest($"Slug '{variantReq.UrlSlug}' is already in use.", "Variants.UrlSlug"));
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
        product.Wheelbase = command.Wheelbase?.Trim();
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

        var allRequestedOptionValues = command.Variants?
            .SelectMany(v => v.OptionValues ?? [])
                .Where(kvp => int.TryParse(kvp.Key, out _) && !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => new { OptionId = int.Parse(kvp.Key), Name = kvp.Value.Trim() })
                .Distinct()
                .ToList() ??
            [];

        var optionIdsToFetch = allRequestedOptionValues.Select(x => x.OptionId).Distinct().ToList();
        var namesToFetch = allRequestedOptionValues.Select(x => x.Name).Distinct().ToList();

        var existingOptionValues = await optionValueReadRepository.GetByIdAndNameAsync(
            optionIdsToFetch,
            namesToFetch,
            cancellationToken)
            .ConfigureAwait(false);

        var inputVariants = command.Variants ?? [];
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
                if(variantEntity == null)
                    continue;
            } else
            {
                variantEntity = new ProductVariant { ProductId = command.Id };
                product.ProductVariants.Add(variantEntity);
            }

            variantEntity.UrlSlug = variantReq.UrlSlug?.Trim();
            variantEntity.Price = variantReq.Price;
            variantEntity.CoverImageUrl = variantReq.CoverImageUrl?.Trim();

            UpdateVariantPhotos(variantEntity, variantReq.PhotoCollection);

            await ProcessOptionValuesInMemoryAsync(
                variantEntity,
                variantReq.OptionValues,
                existingOptionValues,
                optionValueInsertRepository,
                unitOfWork,
                cancellationToken)
                .ConfigureAwait(false);
        }

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = product.Adapt<ProductDetailForManagerResponse>();
        return response;
    }

    private static async Task ProcessOptionValuesInMemoryAsync(
        ProductVariant variant,
        Dictionary<string, string>? requestOptionValues,
        List<OptionValueEntity> preLoadedOptionValues,
        IOptionValueInsertRepository insertRepo,
        IUnitOfWork uow,
        CancellationToken ct)
    {
        if(requestOptionValues == null || requestOptionValues.Count == 0)
            return;

        var targetOptionValueIds = new HashSet<int>();

        foreach(var kvp in requestOptionValues)
        {
            if(!int.TryParse(kvp.Key, out var optionId))
                continue;
            var name = kvp.Value?.Trim();
            if(string.IsNullOrWhiteSpace(name))
                continue;

            var match = preLoadedOptionValues.FirstOrDefault(
                ov => ov.OptionId == optionId && string.Equals(ov.Name, name, StringComparison.OrdinalIgnoreCase));

            if(match != null)
            {
                targetOptionValueIds.Add(match.Id);
            } else
            {
                var newOv = new OptionValueEntity { OptionId = optionId, Name = name };
                insertRepo.Add(newOv);
                await uow.SaveChangesAsync(ct).ConfigureAwait(false);

                targetOptionValueIds.Add(newOv.Id);
                preLoadedOptionValues.Add(newOv);
            }
        }

        var currentLinks = variant.VariantOptionValues.ToList();

        var toRemove = currentLinks.Where(
            l => l.OptionValueId.HasValue && !targetOptionValueIds.Contains(l.OptionValueId.Value))
            .ToList();
        foreach(var item in toRemove)
        {
            variant.VariantOptionValues.Remove(item);
        }

        var currentIds = currentLinks.Where(l => l.OptionValueId.HasValue)
            .Select(l => l.OptionValueId!.Value)
            .ToHashSet();
        foreach(var targetId in targetOptionValueIds)
        {
            if(!currentIds.Contains(targetId))
            {
                variant.VariantOptionValues.Add(new VariantOptionValue { OptionValueId = targetId });
            }
        }
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
}