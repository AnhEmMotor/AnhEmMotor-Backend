using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed record RestoreManyProductsCommand(List<int> Ids) : IRequest<Result<List<ProductDetailForManagerResponse>?>>;
