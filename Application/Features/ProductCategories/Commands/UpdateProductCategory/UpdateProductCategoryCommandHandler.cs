using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Helpers;
using Mapster;
using MediatR;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryCommandHandler(
    IProductCategoryReadRepository readRepository,
    IProductCategoryUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCategoryCommand, (ProductCategoryResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> Handle(
        UpdateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if(category == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [ new ErrorDetail { Message = $"Product category with Id {request.Id} not found." } ]
            });
        }

        request.Adapt(category);

        updateRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (category.Adapt<ProductCategoryResponse>(), null);
    }
}
