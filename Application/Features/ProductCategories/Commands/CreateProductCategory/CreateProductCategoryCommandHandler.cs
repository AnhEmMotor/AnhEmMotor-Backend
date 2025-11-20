using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed class CreateProductCategoryCommandHandler(IProductCategoryInsertRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCategoryCommand, ProductCategoryResponse>
{
    public async Task<ProductCategoryResponse> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ProductCategoryEntity
        {
            Name = request.Name,
            Description = request.Description
        };

        repository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new ProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}
