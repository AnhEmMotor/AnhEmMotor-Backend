using MediatR;

namespace Application.Features.Products.Commands.UpdateProductPrice;

public sealed record UpdateProductPriceCommand(int Id, long Price) : IRequest<(ApiContracts.Product.Responses.ProductDetailForManagerResponse? Data, Common.Models.ErrorResponse? Error)>;
