using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInputStatusList;

public sealed record GetInputStatusListQuery : IRequest<Result<Dictionary<string, string>>>;
