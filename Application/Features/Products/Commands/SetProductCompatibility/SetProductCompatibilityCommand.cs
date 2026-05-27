using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Products.Commands.SetProductCompatibility;

public sealed record SetProductCompatibilityCommand : IRequest<Result<Unit>>
{
    public int ProductId { get; init; }

    [JsonPropertyName("compatible_vehicle_ids")]
    public List<int> CompatibleVehicleIds { get; init; } = [];
}

