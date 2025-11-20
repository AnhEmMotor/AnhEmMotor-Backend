using Application.ApiContracts.ProductCategory;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed record UpdateProductCategoryCommand(int Id, string? Name, string? Description) : IRequest<(ProductCategoryResponse? Data, ErrorResponse? Error)>;
