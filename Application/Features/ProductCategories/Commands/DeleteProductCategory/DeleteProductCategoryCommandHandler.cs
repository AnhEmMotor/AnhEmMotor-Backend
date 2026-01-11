using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;

using MediatR;

namespace Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed class DeleteProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCategoryCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(category == null)
        {
            return Result.Failure(Error.NotFound($"Product category with Id {request.Id} not found."));
        }

        deleteRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
