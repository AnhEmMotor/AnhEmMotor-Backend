using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductPrice;

public sealed record UpdateProductPriceCommand(int Id, decimal Price) : IRequest<Result<ProductDetailForManagerResponse?>>;
