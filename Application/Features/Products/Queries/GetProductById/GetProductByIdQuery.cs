using Application.ApiContracts.Product.Select;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id, bool IncludeDeleted = false)
    : IRequest<(ProductDetailResponse? Data, ErrorResponse? Error)>;
