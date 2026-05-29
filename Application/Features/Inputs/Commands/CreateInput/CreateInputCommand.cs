using Application.ApiContracts.InventoryReceipt.Requests;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.CreateInput;

public sealed record CreateInputCommand : IRequest<Result<InputDetailResponse?>>
{
    public string? Notes { get; init; }

    public string? StatusId { get; init; }

    public int? PurchaseRequestId { get; init; }

    public List<CreateInputInfoRequest> Products { get; init; } = [];
}
