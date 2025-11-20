using Application.ApiContracts.Product.Delete;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.File;
using Application.Interfaces.Services.Product;
using Domain.Helpers;
using ProductEntity = Domain.Entities.Product;

namespace Application.Services.Product;

public class ProductDeleteService(
    IProductSelectRepository selectRepository,
    IProductDeleteRepository deleteRepository,
    IFileDeleteService fileDeleteService,
    IUnitOfWork unitOfWork) : IProductDeleteService
{
    public async Task<ErrorResponse?> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await selectRepository.GetProductWithDetailsByIdAsync(id, includeDeleted: false, cancellationToken).ConfigureAwait(false);

        if (product == null)
        {
            return new ErrorResponse { Errors = [new ErrorDetail { Message = $"Product with Id {id} not found." }] };
        }

        var imageUrls = CollectImageUrls([product]);

        deleteRepository.Delete(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (imageUrls != null && imageUrls.Count > 0)
        {
            await fileDeleteService.DeleteMultipleFilesAsync(imageUrls, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    public async Task<ErrorResponse?> DeleteProductsAsync(DeleteManyProductsRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return null;
        }

        var uniqueIds = request.Ids.Distinct().ToList();

        var activeProducts = await selectRepository.GetActiveProductsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
        var allProducts = await selectRepository.GetAllProductsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

        var allProductsMap = allProducts.ToDictionary(p => p.Id!);
        var activeProductsSet = activeProducts.Select(p => p.Id!).ToHashSet();

        var errorDetails = new List<ErrorDetail>();

        foreach (var id in uniqueIds)
        {
            if (!allProductsMap.ContainsKey(id))
            {
                errorDetails.Add(new ErrorDetail { Message = "Product not found", Field = $"Product ID: {id}" });
                continue;
            }

            if (!activeProductsSet.Contains(id))
            {
                var productName = allProductsMap[id].Name;
                errorDetails.Add(new ErrorDetail { Message = "Product has already been deleted", Field = productName });
            }
        }

        if (errorDetails.Count > 0)
        {
            return new ErrorResponse { Errors = errorDetails };
        }

        if (activeProducts.Count > 0)
        {
            var imageUrls = CollectImageUrls(activeProducts);

            deleteRepository.Delete(activeProducts);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (imageUrls != null && imageUrls.Count > 0)
            {
                await fileDeleteService.DeleteMultipleFilesAsync(imageUrls, cancellationToken).ConfigureAwait(false);
            }
        }

        return null;
    }

    private static List<string?>? CollectImageUrls(List<ProductEntity> products)
    {
        var urls = new HashSet<string>();

        foreach (var product in products)
        {
            if (product.ProductVariants == null) continue;

            foreach (var variant in product.ProductVariants)
            {
                if (!string.IsNullOrWhiteSpace(variant.CoverImageUrl))
                {
                    urls.Add(variant.CoverImageUrl);
                }

                if (variant.ProductCollectionPhotos != null)
                {
                    foreach (var photo in variant.ProductCollectionPhotos)
                    {
                        if (!string.IsNullOrWhiteSpace(photo.ImageUrl))
                        {
                            urls.Add(photo.ImageUrl);
                        }
                    }
                }
            }
        }
        return [.. urls];
    }
}