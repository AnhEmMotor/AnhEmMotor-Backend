using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Commands.CloneInput;

public sealed record CloneInputCommand : IRequest<Result<InputDetailResponse?>>
{
    public int? Id { get; init; }
}
