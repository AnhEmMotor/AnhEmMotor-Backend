using Application.ApiContracts.ProductCategory.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryById;

public sealed record GetProductCategoryByIdQuery(int Id) : IRequest<(ProductCategoryResponse? Data, ErrorResponse? Error)>;
