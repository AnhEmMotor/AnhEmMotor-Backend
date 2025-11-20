using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IProductSelectRepository selectRepository,
    IProductDeleteRepository deleteRepository,
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

        deleteRepository.Delete(product);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
