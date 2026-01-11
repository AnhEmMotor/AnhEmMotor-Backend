
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed record UpdateProductStatusCommand(int Id, string StatusId) : IRequest<Result<ProductDetailForManagerResponse?>>;
