using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IRequest<(ApiContracts.Product.Responses.ProductDetailResponse? Data, ErrorResponse? Error)>;
