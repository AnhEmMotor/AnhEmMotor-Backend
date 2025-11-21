using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed record UpdateManyVariantPricesCommand(List<int> Ids, long Price) : IRequest<(List<int>? Data, ErrorResponse? Error)>;
