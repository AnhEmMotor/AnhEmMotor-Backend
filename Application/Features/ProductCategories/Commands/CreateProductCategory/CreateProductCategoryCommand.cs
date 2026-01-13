using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand : IRequest<Result<ProductCategoryResponse>>
{
    public string? Name { get; init; }

    public string? Description { get; init; }
}
