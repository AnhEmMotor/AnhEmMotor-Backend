using Application.ApiContracts.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductPrice;

public sealed record UpdateProductPriceCommand(int Id, long Price) : IRequest<(ProductDetailResponse? Data, ErrorResponse? Error)>;
