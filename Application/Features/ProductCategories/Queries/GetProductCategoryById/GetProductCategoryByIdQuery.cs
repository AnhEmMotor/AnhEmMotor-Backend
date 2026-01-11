using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryById;

public sealed record GetProductCategoryByIdQuery(int Id) : IRequest<Result<ProductCategoryResponse?>>;
