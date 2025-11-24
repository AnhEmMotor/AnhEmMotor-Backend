using Application.ApiContracts.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed record RestoreProductCategoryCommand : IRequest<(ProductCategoryResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }
}
