using Application.ApiContracts.Product.Delete;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.File;
using Application.Interfaces.Services.Product;
using Domain.Helpers;

namespace Application.Services.Product;

public class ProductDeleteService(
    IProductSelectRepository selectRepository,
    IProductUpdateRepository updateRepository,
    IProductDeleteRepository deleteRepository,
    IFileDeleteService fileDeleteService) : IProductDeleteService
{
    public async Task<ErrorResponse?> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await selectRepository.GetProductWithDetailsByIdAsync(id, includeDeleted: false, cancellationToken).ConfigureAwait(false);
        if (product == null)
        {
            return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Product with Id {id} not found." }] };
        }

        var imageUrls = new List<string>();
        foreach (var variant in product.ProductVariants)
        {
            if (!string.IsNullOrWhiteSpace(variant.CoverImageUrl))
            {
                imageUrls.Add(variant.CoverImageUrl);
            }

            foreach (var photo in variant.ProductCollectionPhotos)
            {
                if (!string.IsNullOrWhiteSpace(photo.ImageUrl))
                {
                    imageUrls.Add(photo.ImageUrl);
                }
            }
        }

        await deleteRepository.DeleteProductAsync(product, cancellationToken).ConfigureAwait(false);

        foreach (var imageUrl in imageUrls.Distinct())
        {
            await fileDeleteService.DeleteFileAsync(imageUrl, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    public async Task<ErrorResponse?> DeleteProductsAsync(DeleteManyProductsRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var errorDetails = new List<ErrorDetail>();
        var activeProducts = await selectRepository.GetActiveProductsByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);
        var allProducts = await selectRepository.GetAllProductsByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        foreach (var id in request.Ids)
        {
            var product = allProducts.FirstOrDefault(p => p.Id == id);
            var activeProduct = activeProducts.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                errorDetails.Add(new ErrorDetail { Message = "Product not found", Field = $"Product ID: {id}" });
            }
            else if (activeProduct == null)
            {
                errorDetails.Add(new ErrorDetail { Message = "Product has already been deleted", Field = product.Name });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        var imageUrls = new List<string>();
        foreach (var product in activeProducts)
        {
            foreach (var variant in product.ProductVariants)
            {
                if (!string.IsNullOrWhiteSpace(variant.CoverImageUrl))
                {
                    imageUrls.Add(variant.CoverImageUrl);
                }

                foreach (var photo in variant.ProductCollectionPhotos)
                {
                    if (!string.IsNullOrWhiteSpace(photo.ImageUrl))
                    {
                        imageUrls.Add(photo.ImageUrl);
                    }
                }
            }
        }

        if (activeProducts.Count > 0)
        {
            await deleteRepository.DeleteProductsAsync(activeProducts, cancellationToken).ConfigureAwait(false);
        }

        foreach (var imageUrl in imageUrls.Distinct())
        {
            await fileDeleteService.DeleteFileAsync(imageUrl, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    public async Task<ErrorResponse?> RestoreProductAsync(int id, CancellationToken cancellationToken)
    {
        var deletedProducts = await selectRepository.GetDeletedProductsByIdsAsync([id], cancellationToken).ConfigureAwait(false);
        if (deletedProducts.Count == 0)
        {
            return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Deleted product with Id {id} not found." }] };
        }

        await updateRepository.RestoreAsync(deletedProducts[0], cancellationToken).ConfigureAwait(false);

        return null;
    }

    public async Task<ErrorResponse?> RestoreProductsAsync(RestoreManyProductsRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var errorDetails = new List<ErrorDetail>();
        var deletedProducts = await selectRepository.GetDeletedProductsByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);
        var allProducts = await selectRepository.GetAllProductsByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

        foreach (var id in request.Ids)
        {
            var product = allProducts.FirstOrDefault(p => p.Id == id);
            var deletedProduct = deletedProducts.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                errorDetails.Add(new ErrorDetail { Message = "Product not found", Field = $"Product ID: {id}" });
            }
            else if (deletedProduct == null)
            {
                errorDetails.Add(new ErrorDetail { Message = "Product is not deleted", Field = product.Name });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (deletedProducts.Count > 0)
        {
            await updateRepository.RestoreAsync(deletedProducts, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}
