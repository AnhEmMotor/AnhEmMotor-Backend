using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(int Id, UpdateProductRequest Request) : IRequest<Result<ProductDetailForManagerResponse?>>;
