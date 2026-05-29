using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.RestoreInput;

public sealed record RestoreInputCommand : IRequest<Result<InputDetailResponse>>
{
    public int? Id { get; init; }
}
