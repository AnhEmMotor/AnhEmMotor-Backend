using Application.ApiContracts.ProductCategory.Responses;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed record RestoreProductCategoryCommand : IRequest<(ProductCategoryResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }
}
