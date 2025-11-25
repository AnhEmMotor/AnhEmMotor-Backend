using Application.ApiContracts.Product;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed record UpdateProductStatusCommand(int Id, string StatusId) : IRequest<(ProductDetailResponse? Data, ErrorResponse? Error)>;
