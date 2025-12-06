using Application.ApiContracts.ProductCategory.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed record RestoreManyProductCategoriesCommand : IRequest<(List<ProductCategoryResponse>? Data, ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
}
