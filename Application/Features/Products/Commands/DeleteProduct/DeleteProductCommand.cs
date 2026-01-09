using Domain.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(int Id) : IRequest<Common.Models.ErrorResponse?>;
