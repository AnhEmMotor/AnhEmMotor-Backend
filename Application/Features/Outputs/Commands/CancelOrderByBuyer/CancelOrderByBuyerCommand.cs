using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Outputs.Commands.CancelOrderByBuyer;

public sealed record CancelOrderByBuyerCommand : IRequest<Result<OrderDetailResponse>>
{
    public int Id { get; init; }

    [JsonIgnore]
    public Guid? CurrentUserId { get; init; }
}
