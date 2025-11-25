using Application.Common.Extensions;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MediaFile;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.VariantOptionValue;
using Domain.Helpers;
using MediatR;
using Domain.Enums;
using Application.ApiContracts.Product;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed class RestoreProductCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreProductCommand, (ProductDetailResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductDetailResponse? Data, ErrorResponse? Error)> Handle(RestoreProductCommand command, CancellationToken cancellationToken)
    {
        var deletedProduct = await readRepository.GetByIdAsync(command.Id, cancellationToken, DataFetchMode.DeletedOnly).ConfigureAwait(false);
        if (deletedProduct == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Deleted product with Id {command.Id} not found." }]
            });
        }

        var imageFileNames = new List<string>();
        foreach (var variant in deletedProduct.ProductVariants)
        {
            if (!string.IsNullOrWhiteSpace(variant.CoverImageUrl))
            {
                imageFileNames.Add(StringExtensions.ExtractFileName(variant.CoverImageUrl));
            }

            foreach (var photo in variant.ProductCollectionPhotos)
            {
                if (!string.IsNullOrWhiteSpace(photo.ImageUrl))
                {
                    imageFileNames.Add(StringExtensions.ExtractFileName(photo.ImageUrl));
                }
            }
        }

        updateRepository.Restore(deletedProduct);

        // Cascade restore associated MediaFile records
        // Note: This functionality needs to be implemented when MediaFile filtering by StoragePath is added
        // For now, files will remain in their current state when products are restored

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = ProductResponseMapper.BuildProductDetailResponse(deletedProduct);
        return (response, null);
    }
}
