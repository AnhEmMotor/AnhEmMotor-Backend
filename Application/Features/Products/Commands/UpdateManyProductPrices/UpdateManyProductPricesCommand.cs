using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed record UpdateManyProductPricesCommand(List<int> Ids, long Price) : IRequest<(List<int>? Data, Common.Models.ErrorResponse? Error)>;
