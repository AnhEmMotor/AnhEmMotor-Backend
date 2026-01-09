using Domain.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IRequest<(ApiContracts.Product.Responses.ProductDetailForManagerResponse? Data, Common.Models.ErrorResponse? Error)>;
