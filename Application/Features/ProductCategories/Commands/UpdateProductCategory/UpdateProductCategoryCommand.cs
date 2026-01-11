using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed record UpdateProductCategoryCommand : IRequest<Result<ProductCategoryResponse?>>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}
