using Application.ApiContracts.Product.Common;
using MediatR;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed record UpdateVariantPriceCommand(int VariantId, long Price) : IRequest<(ProductVariantLiteResponse? Data, Common.Models.ErrorResponse? Error)>;
