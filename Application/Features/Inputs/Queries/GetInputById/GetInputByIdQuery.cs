using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInputById;

public sealed record GetInputByIdQuery : IRequest<Result<InputDetailResponse?>>
{
    public int Id { get; init; }
}
