using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStatusList;

public sealed record GetInventoryReceiptStatusListQuery : IRequest<Result<Dictionary<string, string>>>;
