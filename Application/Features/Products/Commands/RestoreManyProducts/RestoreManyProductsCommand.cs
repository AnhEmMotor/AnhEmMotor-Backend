using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed record RestoreManyProductsCommand(List<int> Ids) : IRequest<(List<ApiContracts.Product.Responses.ProductDetailResponse>? Data, ErrorResponse? Error)>;
