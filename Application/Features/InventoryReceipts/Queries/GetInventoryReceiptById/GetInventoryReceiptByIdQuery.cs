using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptById;

public sealed record GetInventoryReceiptByIdQuery : IRequest<Result<InventoryReceiptDetailResponse?>>
{
    [JsonIgnore]
    public int Id { get; init; }
}
