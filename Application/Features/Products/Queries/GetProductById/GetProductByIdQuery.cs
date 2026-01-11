using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IRequest<Result<ProductDetailForManagerResponse?>>;
