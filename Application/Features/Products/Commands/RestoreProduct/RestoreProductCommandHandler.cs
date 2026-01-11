using Application.ApiContracts.Product.Responses;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using Domain.Constants;
using Mapster;
using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed class RestoreProductCommandHandler(
    IProductReadRepository readRepository,
    IProductUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RestoreProductCommand, Result<ProductDetailForManagerResponse?>>
{
    public async Task<Result<ProductDetailForManagerResponse?>> Handle(
        RestoreProductCommand command,
        CancellationToken cancellationToken)
    {
        var deletedProduct = await readRepository.GetByIdAsync(command.Id, cancellationToken, DataFetchMode.DeletedOnly)
            .ConfigureAwait(false);
        if(deletedProduct == null)
        {
            return Error.NotFound($"Deleted product with Id {command.Id} not found.");
        }

        var imageFileNames = new List<string>();
        foreach(var variant in deletedProduct.ProductVariants)
        {
            if(!string.IsNullOrWhiteSpace(variant.CoverImageUrl))
            {
                imageFileNames.Add(StringExtensions.ExtractFileName(variant.CoverImageUrl));
            }

            foreach(var photo in variant.ProductCollectionPhotos)
            {
                if(!string.IsNullOrWhiteSpace(photo.ImageUrl))
                {
                    imageFileNames.Add(StringExtensions.ExtractFileName(photo.ImageUrl));
                }
            }
        }

        updateRepository.Restore(deletedProduct);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = deletedProduct.Adapt<ProductDetailForManagerResponse>();
        return response;
    }
}
