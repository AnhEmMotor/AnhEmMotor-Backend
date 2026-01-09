using Domain.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed record RestoreProductCommand(int Id) : IRequest<(ApiContracts.Product.Responses.ProductDetailForManagerResponse? Data, Common.Models.ErrorResponse? Error)>;
