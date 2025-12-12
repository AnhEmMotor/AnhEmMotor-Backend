using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(int Id, ApiContracts.Product.Requests.UpdateProductRequest Request) : IRequest<(ApiContracts.Product.Responses.ProductDetailForManagerResponse? Data, Common.Models.ErrorResponse? Error)>;
