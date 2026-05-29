using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.RestoreManyInputs;

public sealed record RestoreManyInputsCommand : IRequest<Result<List<InputDetailResponse>>>
{
    public ICollection<int> Ids { get; init; } = [];
}
