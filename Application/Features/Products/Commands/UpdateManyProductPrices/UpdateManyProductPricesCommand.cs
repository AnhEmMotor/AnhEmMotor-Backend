using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed record UpdateManyProductPricesCommand(Dictionary<string, long?> ProductPrices) : IRequest<(List<int>? Data, ErrorResponse? Error)>;
