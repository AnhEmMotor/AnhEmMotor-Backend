using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.File;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductSelectRepository selectRepository,
    IProductDeleteRepository deleteRepository,
    IMediaFileSelectRepository mediaFileSelectRepository,
    IMediaFileDeleteRepository mediaFileDeleteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await selectRepository.GetProductWithDetailsByIdAsync(command.Id, includeDeleted: false, cancellationToken).ConfigureAwait(false);

        if (product == null)
        {
            return new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product with Id {command.Id} not found." }]
            };
        }

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

        deleteRepository.Delete(product);

        // Cascade delete associated MediaFile records (soft delete)
        if (imageFileNames.Count > 0)
        {
            var distinctFileNames = imageFileNames.Distinct().ToList();
            var mediaFiles = await mediaFileSelectRepository.GetByStoredFileNamesAsync(distinctFileNames.ConvertAll<string?>(x => x), cancellationToken, includeDeleted: false).ConfigureAwait(false);
            
            if (mediaFiles != null && mediaFiles.Count > 0)
            {
                foreach (var mediaFile in mediaFiles)
                {
                    mediaFileDeleteRepository.Delete(mediaFile);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
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
