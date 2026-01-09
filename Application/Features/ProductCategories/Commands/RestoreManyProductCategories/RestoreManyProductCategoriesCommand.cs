using Application.ApiContracts.ProductCategory.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed record RestoreManyProductCategoriesCommand : IRequest<(List<ProductCategoryResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public List<int> Ids { get; init; } = [];
}
