using Application.ApiContracts.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed record RestoreManyProductsCommand(List<int> Ids) : IRequest<(List<ProductDetailResponse>? Data, ErrorResponse? Error)>;
