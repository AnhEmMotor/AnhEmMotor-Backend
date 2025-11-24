using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed class RestoreProductCommandHandler(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreProductCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(RestoreProductCommand command, CancellationToken cancellationToken)
    {
        var deletedProducts = await selectRepository.GetDeletedProductsByIdsAsync([command.Id], cancellationToken).ConfigureAwait(false);
        if (deletedProducts.Count == 0)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Deleted product with Id {command.Id} not found." }]
            });
        }

        var product = deletedProducts[0];

        // Collect all image URLs from variants
        var imageFileNames = new List<string>();
        foreach (var variant in product.ProductVariants)
        {
            if (!string.IsNullOrWhiteSpace(variant.CoverImageUrl))
            {
                imageFileNames.Add(ExtractFileName(variant.CoverImageUrl));
            }

            foreach (var photo in variant.ProductCollectionPhotos)
            {
                if (!string.IsNullOrWhiteSpace(photo.ImageUrl))
                {
                    imageFileNames.Add(ExtractFileName(photo.ImageUrl));
                }
            }
        }

        // Restore product and cascade restore all variants
        updateRepository.Restore(product);

        // Cascade restore associated MediaFile records
        // Note: This functionality needs to be implemented when MediaFile filtering by StoragePath is added
        // For now, files will remain in their current state when products are restored

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = ProductResponseMapper.BuildProductDetailResponse(product);
        return (response, null);
    }

    private static string ExtractFileName(string urlOrFileName)
    {
        // Extract filename from URL like "/uploads/guid.webp" or just return "guid.webp"
        var fileName = urlOrFileName.Trim();
        if (fileName.Contains('/'))
        {
            fileName = fileName.Split('/').Last();
        }
        return fileName;
    }
}
