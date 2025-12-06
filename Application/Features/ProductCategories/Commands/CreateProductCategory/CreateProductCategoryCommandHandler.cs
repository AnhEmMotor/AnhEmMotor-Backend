using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Mapster;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed class CreateProductCategoryCommandHandler(
    IProductCategoryInsertRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCategoryCommand, ProductCategoryResponse>
{
    public async Task<ProductCategoryResponse> Handle(
        CreateProductCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = request.Adapt<ProductCategoryEntity>();

        repository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return category.Adapt<ProductCategoryResponse>();
    }
}
