using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed record UpdateManyVariantPricesCommand(Dictionary<int, long> VariantPrices) : IRequest<(List<int>? Data, ErrorResponse? Error)>;
