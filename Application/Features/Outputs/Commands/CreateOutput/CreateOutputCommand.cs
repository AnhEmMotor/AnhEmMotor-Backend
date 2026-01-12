using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed record CreateOutputCommand : IRequest<Result<OutputResponse?>>
{
    public Guid? BuyerId { get; init; }

    public string? Notes { get; init; }

    [JsonPropertyName("products")]
    public List<CreateOutputInfoRequest> OutputInfos { get; init; } = [];
}