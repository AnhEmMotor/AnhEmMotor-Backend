using Application.ApiContracts.ProductCategory.Responses;

using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoryById;

public sealed record GetProductCategoryByIdQuery(int Id) : IRequest<(ProductCategoryResponse? Data, Common.Models.ErrorResponse? Error)>;
