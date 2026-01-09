using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductReadRepository readRepository,
    IProductDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, Common.Models.ErrorResponse?>
{
    public async Task<Common.Models.ErrorResponse?> Handle(
        DeleteProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdWithDetailsAsync(command.Id, cancellationToken).ConfigureAwait(false);

        if(product == null)
        {
            return new Common.Models.ErrorResponse
            {
                Errors = [ new Common.Models.ErrorDetail { Message = $"Product with Id {command.Id} not found." } ]
            };
        }

        var imageFileNames = new List<string>();
        foreach(var variant in product.ProductVariants)
        {
            if(!string.IsNullOrWhiteSpace(variant.CoverImageUrl))
            {
                imageFileNames.Add(ExtractFileName(variant.CoverImageUrl));
            }

            foreach(var photo in variant.ProductCollectionPhotos)
            {
                if(!string.IsNullOrWhiteSpace(photo.ImageUrl))
                {
                    imageFileNames.Add(ExtractFileName(photo.ImageUrl));
                }
            }
        }

        deleteRepository.Delete(product);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }

    private static string ExtractFileName(string urlOrFileName)
    {
        var fileName = urlOrFileName.Trim();
        if(fileName.Contains('/'))
        {
            fileName = fileName.Split('/').Last();
        }
        return fileName;
    }
}
