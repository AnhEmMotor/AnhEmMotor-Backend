using Application.ApiContracts.Product.Select;
using Application.Features.Products.Common;
using Application.Interfaces.Repositories;
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
        updateRepository.Restore(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = ProductResponseMapper.BuildProductDetailResponse(product);
        return (response, null);
    }
}
