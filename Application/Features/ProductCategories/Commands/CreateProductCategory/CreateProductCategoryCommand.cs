using Application.ApiContracts.ProductCategory;
using MediatR;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand(string? Name, string? Description) : IRequest<ProductCategoryResponse>;
