using Application.ApiContracts.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.RestoreProductCategory;

public sealed record RestoreProductCategoryCommand(int Id) : IRequest<(ProductCategoryResponse? Data, ErrorResponse? Error)>;
