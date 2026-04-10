using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderStatusMap;

public sealed record GetOrderStatusMapQuery : IRequest<Result<IEnumerable<object>>>;
