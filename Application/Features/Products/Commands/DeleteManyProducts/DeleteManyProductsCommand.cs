
using MediatR;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed record DeleteManyProductsCommand(List<int> Ids) : IRequest<Common.Models.ErrorResponse?>;
