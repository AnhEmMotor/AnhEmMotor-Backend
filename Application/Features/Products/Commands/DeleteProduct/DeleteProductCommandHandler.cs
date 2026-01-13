using Application.Common.Extensions;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;

using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductReadRepository readRepository,
    IProductDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken).ConfigureAwait(false);

        if(product == null)
        {
            return Result.Failure(Error.NotFound($"Product with Id {command.Id} not found."));
        }

        var imageFileNames = new List<string>();
        foreach(var variant in product.ProductVariants)
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

        deleteRepository.Delete(product);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
