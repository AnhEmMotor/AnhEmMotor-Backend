using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductStatuses;

public sealed record UpdateManyProductStatusesCommand(List<int> Ids, string StatusId) : IRequest<(List<int>? Data, ErrorResponse? Error)>;
