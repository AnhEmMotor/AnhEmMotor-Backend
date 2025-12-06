using Application.ApiContracts.ProductCategory.Responses;
using MediatR;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand : IRequest<ProductCategoryResponse>
{
    public string? Name { get; init; }

    public string? Description { get; init; }
}
