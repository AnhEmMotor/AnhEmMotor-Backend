using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderCancellableStatuses;

public sealed record GetOrderCancellableStatusesQuery : IRequest<Result<IEnumerable<string>>>;
