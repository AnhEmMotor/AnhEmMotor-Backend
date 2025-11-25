using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed record RestoreProductCommand(int Id) : IRequest<(ApiContracts.Product.Responses.ProductDetailResponse? Data, ErrorResponse? Error)>;
