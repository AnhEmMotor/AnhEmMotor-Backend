using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed class DeleteProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCategoryCommand, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(category == null)
        {
            return new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Product category with Id {request.Id} not found." } ]
            };
        }

        deleteRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return null;
    }
}
