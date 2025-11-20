using Application.ApiContracts.Product.Select;
using Application.ApiContracts.Product.Update;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(int Id, UpdateProductRequest Request) : IRequest<(ProductDetailResponse? Data, ErrorResponse? Error)>;
