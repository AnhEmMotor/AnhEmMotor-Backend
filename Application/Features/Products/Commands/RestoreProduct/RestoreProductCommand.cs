using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed record RestoreProductCommand(int Id) : IRequest<(ApiContracts.Product.Responses.ProductDetailResponse? Data, Common.Models.ErrorResponse? Error)>;
