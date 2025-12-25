using MediatR;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed record UpdateManyVariantPricesCommand(List<int> Ids, decimal Price) : IRequest<(List<int>? Data, Common.Models.ErrorResponse? Error)>;
