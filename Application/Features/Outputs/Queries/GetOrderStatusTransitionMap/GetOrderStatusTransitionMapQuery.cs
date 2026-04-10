using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOrderStatusTransitionMap;

public sealed record GetOrderStatusTransitionMapQuery : IRequest<Result<Dictionary<string, HashSet<string>>>>;
