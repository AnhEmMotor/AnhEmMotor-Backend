
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed record UpdateManyProductPricesCommand(List<int> Ids, decimal Price) : IRequest<Result<List<int>?>>;
