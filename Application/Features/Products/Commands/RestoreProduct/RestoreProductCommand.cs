
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed record RestoreProductCommand(int Id) : IRequest<Result<ProductDetailForManagerResponse?>>;
