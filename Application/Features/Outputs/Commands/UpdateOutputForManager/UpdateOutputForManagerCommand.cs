using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public sealed record UpdateOutputForManagerCommand : IRequest<Result<OrderDetailResponse>>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }
    
    public Guid? BuyerId { get; init; }

    public Guid? CurrentUserId { get; init; }

    public string? Notes { get; init; }

    [JsonPropertyName("products")]
    public ICollection<UpdateOutputInfoRequest> OutputInfos { get; init; } = [];
}
