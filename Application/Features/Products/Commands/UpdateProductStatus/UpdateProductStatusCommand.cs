using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed record UpdateProductStatusCommand(int Id, string StatusId) : IRequest<(ApiContracts.Product.Responses.ProductDetailResponse? Data, Common.Models.ErrorResponse? Error)>;
