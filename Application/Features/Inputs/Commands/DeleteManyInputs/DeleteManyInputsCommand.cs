
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.DeleteManyInputs;

public sealed record DeleteManyInputsCommand : IRequest<Result>
{
    public ICollection<int> Ids { get; init; } = [];
}
