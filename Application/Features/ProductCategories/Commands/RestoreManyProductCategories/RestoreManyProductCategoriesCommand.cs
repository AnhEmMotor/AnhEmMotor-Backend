using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed record RestoreManyProductCategoriesCommand : IRequest<Result<List<ProductCategoryResponse>?>>
{
    public List<int> Ids { get; init; } = [];
}
