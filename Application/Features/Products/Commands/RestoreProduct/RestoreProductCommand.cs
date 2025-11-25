using Application.ApiContracts.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed record RestoreProductCommand(int Id) : IRequest<(ProductDetailResponse? Data, ErrorResponse? Error)>;
