using Application.ApiContracts.Product.Responses;

using MediatR;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed record UpdateVariantPriceCommand(int VariantId, decimal Price) : IRequest<(ProductVariantLiteResponse? Data, Common.Models.ErrorResponse? Error)>;
