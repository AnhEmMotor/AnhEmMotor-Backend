using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryCommandHandler(IProductCategorySelectRepository selectRepository, IProductCategoryUpdateRepository updateRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProductCategoryCommand, (ProductCategoryResponse? Data, ErrorResponse? Error)>
{
    public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await selectRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (category == null)
        {
            return (null, new ErrorResponse
            {
                Errors = [new ErrorDetail { Message = $"Product category with Id {request.Id} not found." }]
            });
        }

        if (request.Name is not null)
        {
            category.Name = request.Name;
        }

        if (request.Description is not null)
        {
            category.Description = request.Description;
        }

        updateRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (new ProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        }, null);
    }
}
