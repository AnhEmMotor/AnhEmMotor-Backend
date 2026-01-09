
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed record RestoreManyProductsCommand(List<int> Ids) : IRequest<(List<ApiContracts.Product.Responses.ProductDetailForManagerResponse>? Data, Common.Models.ErrorResponse? Error)>;
