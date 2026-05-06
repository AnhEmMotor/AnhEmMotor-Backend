using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand : IRequest<Result<ProductCategoryResponse>>
{
    public string? Name { get; init; }

    public string? Slug { get; init; }

    public string? ImageUrl { get; init; }

    public bool IsActive { get; init; } = true;

    public int SortOrder { get; init; }

    public int? ParentId { get; init; }

    public string? Description { get; init; }

    public string? CategoryGroup { get; init; }
}
