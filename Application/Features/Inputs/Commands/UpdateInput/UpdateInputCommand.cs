using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.UpdateInput;

public sealed record UpdateInputCommand : IRequest<Result<InputDetailResponse?>>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }

    public int? PurchaseRequestId { get; init; }

    public string? Notes { get; init; }

    public ICollection<UpdateInputInfoRequest> Products { get; init; } = [];
}
