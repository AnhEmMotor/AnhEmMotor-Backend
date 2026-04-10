using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderLockedStatuses;

public sealed record GetOrderLockedStatusesQuery : IRequest<Result<OrderLockStatusResponse>>;
