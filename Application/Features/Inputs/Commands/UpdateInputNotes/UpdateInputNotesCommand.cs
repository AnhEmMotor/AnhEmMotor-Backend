using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInputNotes;

public sealed record UpdateInputNotesCommand : IRequest<Result<InputDetailResponse>>
{
    public int Id { get; init; }

    public string? Notes { get; init; }
}
