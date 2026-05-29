
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteManyInventoryReceipts;

public sealed record DeleteManyInventoryReceiptsCommand : IRequest<Result>
{
    public ICollection<int> Ids { get; init; } = [];
}
