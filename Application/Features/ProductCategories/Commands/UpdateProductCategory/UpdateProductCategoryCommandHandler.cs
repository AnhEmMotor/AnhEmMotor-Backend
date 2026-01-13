using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;

using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCategoryCommand, Result<ProductCategoryResponse?>>
{
    public async Task<Result<ProductCategoryResponse?>> Handle(
        UpdateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(category == null)
        {
            return Error.NotFound($"Product category with Id {request.Id} not found.");
        }

        request.Adapt(category);

        updateRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return category.Adapt<ProductCategoryResponse>();
    }
}
