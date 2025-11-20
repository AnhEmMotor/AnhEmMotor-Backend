using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed record DeleteManyProductsCommand(List<int> Ids) : IRequest<ErrorResponse?>;
