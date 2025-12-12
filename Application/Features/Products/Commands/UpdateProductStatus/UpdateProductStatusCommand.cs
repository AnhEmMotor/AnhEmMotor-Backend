using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed record UpdateProductStatusCommand(int Id, string StatusId) : IRequest<(ApiContracts.Product.Responses.ProductDetailForManagerResponse? Data, Common.Models.ErrorResponse? Error)>;
