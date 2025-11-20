using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Entities;
using Domain.Helpers;
using MediatR;
using ProductEntity = Domain.Entities.Product;

namespace Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductSelectRepository selectRepository, IProductInsertRepository insertRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<ErrorDetail>();

        var category = await selectRepository.GetCategoryByIdAsync(request.CategoryId!.Value, cancellationToken).ConfigureAwait(false);
        if (category == null)
        {
            errors.Add(new ErrorDetail { Field = nameof(request.CategoryId), Message = $"Product category with Id {request.CategoryId} not found." });
        }

        if (request.BrandId.HasValue)
        {
            var brand = await selectRepository.GetBrandByIdAsync(request.BrandId.Value, cancellationToken).ConfigureAwait(false);
            if (brand == null)
            {
                errors.Add(new ErrorDetail { Field = nameof(request.BrandId), Message = $"Brand with Id {request.BrandId} not found." });
            }
        }

        if (request.Variants?.Count > 0)
        {
            var slugs = request.Variants.Select(v => v.UrlSlug?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (slugs.Count != slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                errors.Add(new ErrorDetail { Field = "Variants", Message = "Duplicate slugs found within the request." });
            }

            foreach (var slug in slugs)
            {
                var existing = await selectRepository.GetVariantBySlugAsync(slug!, includeDeleted: true, cancellationToken).ConfigureAwait(false);
                if (existing != null)
                {
                    errors.Add(new ErrorDetail { Field = "Variants.UrlSlug", Message = $"Slug '{slug}' is already in use." });
                }
            }
        }

        var optionValueMap = new Dictionary<int, OptionValue>();
        if (request.Variants?.Count > 0)
        {
            var allOptionValueIds = request.Variants
                .Where(v => v.OptionValueIds?.Count > 0)
                .SelectMany(v => v.OptionValueIds!)
                .Distinct()
                .ToList();

            if (allOptionValueIds.Count > 0)
            {
                var optionValues = await selectRepository.GetOptionValuesByIdsAsync(allOptionValueIds, cancellationToken).ConfigureAwait(false);
                var foundIds = optionValues.Select(ov => ov.Id).ToHashSet();
                var missingIds = allOptionValueIds.Except(foundIds).ToList();
                if (missingIds.Count > 0)
                {
                    errors.Add(new ErrorDetail { Field = "Variants.OptionValueIds", Message = $"Option values not found: {string.Join(", ", missingIds)}." });
                }

                foreach (var ov in optionValues)
                {
                    optionValueMap[ov.Id] = ov;
                }
            }
        }

        if (errors.Count > 0)
        {
            return (null, new ErrorResponse { Errors = errors });
        }

        var product = new ProductEntity
        {
            Name = request.Name?.Trim(),
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Description = request.Description?.Trim(),
            Weight = request.Weight,
            Dimensions = request.Dimensions?.Trim(),
            Wheelbase = request.Wheelbase?.Trim(),
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
            StatusId = request.StatusId?.Trim() ?? "for-sale",
            ProductVariants = []
        };

        if (request.Variants?.Count > 0)
        {
            foreach (var variantReq in request.Variants)
            {
                var variant = new ProductVariant
                {
                    UrlSlug = variantReq.UrlSlug?.Trim(),
                    Price = variantReq.Price,
                    CoverImageUrl = variantReq.CoverImageUrl?.Trim(),
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

                if (variantReq.OptionValueIds?.Count > 0)
                {
                    foreach (var ovId in variantReq.OptionValueIds)
                    {
                        if (optionValueMap.TryGetValue(ovId, out var optionValue))
                        {
                            variant.VariantOptionValues.Add(new VariantOptionValue
                            {
                                OptionValueId = ovId,
                                OptionValue = optionValue
                            });
                        }
                    }
                }

                product.ProductVariants.Add(variant);
            }
        }

        insertRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var created = await selectRepository.GetProductWithDetailsByIdAsync(product.Id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (created == null)
        {
            return (null, new ErrorResponse { Errors = [new ErrorDetail { Message = "Failed to retrieve created product." }] });
        }

        var response = ProductResponseMapper.BuildProductDetailResponse(created);
        return (response, null);
    }
}
