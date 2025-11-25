using Application.ApiContracts.Product;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.OptionValue;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Entities;
using Domain.Helpers;
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
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();
        var request = command.Request;

        var product = await productReadRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken);

        if(product == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Product with Id {command.Id} not found." } ]
            });
        }

        if(request.CategoryId.HasValue)
        {
            var category = await productCategoryReadRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken);
            if(category == null)
            {
                errors.Add(
                    new ErrorDetail
                    {
                        Field = nameof(request.CategoryId),
                        Message = $"Product category with Id {request.CategoryId} not found or has been deleted."
                    });
            }
        }

        if(request.BrandId.HasValue)
        {
            var brand = await brandReadRepository.GetByIdAsync(request.BrandId.Value, cancellationToken)
                .ConfigureAwait(false);
            if(brand == null)
            {
                errors.Add(
                    new ErrorDetail
                    {
                        Field = nameof(request.BrandId),
                        Message = $"Brand with Id {request.BrandId} not found or has been deleted."
                    });
            }
        }

        if(request.Variants?.Count > 0)
        {
            var slugs = request.Variants
                .Select(v => v.UrlSlug?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if(slugs.Count != slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                errors.Add(
                    new ErrorDetail { Field = "Variants", Message = "Duplicate slugs found within the request." });
            }

            foreach(var variantReq in request.Variants.Where(v => !string.IsNullOrWhiteSpace(v.UrlSlug)))
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

                    errors.Add(
                        new ErrorDetail
                        {
                            Field = "Variants.UrlSlug",
                            Message = $"Slug '{variantReq.UrlSlug}' is already in use."
                        });
                }
            }
        }


        if(errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        product.Name = request.Name?.Trim();
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.Description = request.Description?.Trim();
        product.Weight = request.Weight;
        product.Dimensions = request.Dimensions?.Trim();
        product.Wheelbase = request.Wheelbase?.Trim();
        product.SeatHeight = request.SeatHeight;
        product.GroundClearance = request.GroundClearance;
        product.FuelCapacity = request.FuelCapacity;
        product.TireSize = request.TireSize?.Trim();
        product.FrontSuspension = request.FrontSuspension?.Trim();
        product.RearSuspension = request.RearSuspension?.Trim();
        product.EngineType = request.EngineType?.Trim();
        product.MaxPower = request.MaxPower?.Trim();
        product.OilCapacity = request.OilCapacity;
        product.FuelConsumption = request.FuelConsumption?.Trim();
        product.TransmissionType = request.TransmissionType?.Trim();
        product.StarterSystem = request.StarterSystem?.Trim();
        product.MaxTorque = request.MaxTorque?.Trim();
        product.Displacement = request.Displacement;
        product.BoreStroke = request.BoreStroke?.Trim();
        product.CompressionRatio = request.CompressionRatio?.Trim();
        product.StatusId = request.StatusId?.Trim() ?? "for-sale";

        var allRequestedOptionValues = request.Variants?
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
            cancellationToken);

        var inputVariants = request.Variants ?? [];
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
                cancellationToken);
        }

        updateRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = ProductResponseMapper.BuildProductDetailResponse(product);
        return (response, null);
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
                await uow.SaveChangesAsync(ct);

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