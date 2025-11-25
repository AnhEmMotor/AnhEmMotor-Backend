using Application.ApiContracts.Product.Select;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id)
    : IRequest<(ProductDetailResponse? Data, ErrorResponse? Error)>;
