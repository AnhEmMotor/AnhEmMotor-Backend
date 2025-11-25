using Application.ApiContracts.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed record UpdateProductCategoryCommand : IRequest<(ProductCategoryResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }
}
