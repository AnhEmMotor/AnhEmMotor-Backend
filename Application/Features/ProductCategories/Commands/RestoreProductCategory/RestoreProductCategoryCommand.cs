using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed record RestoreProductCategoryCommand : IRequest<Result<ProductCategoryResponse?>>
{
    public int Id { get; init; }
}
