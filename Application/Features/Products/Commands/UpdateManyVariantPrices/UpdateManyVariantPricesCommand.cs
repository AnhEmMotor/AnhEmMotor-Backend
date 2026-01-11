
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed record UpdateManyVariantPricesCommand(List<int> Ids, decimal Price) : IRequest<Result<List<int>>>;
