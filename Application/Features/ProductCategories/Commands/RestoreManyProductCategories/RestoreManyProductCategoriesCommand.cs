using Application.ApiContracts.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed record RestoreManyProductCategoriesCommand(List<int> Ids) : IRequest<(List<ProductCategoryResponse>? Data, ErrorResponse? Error)>;
