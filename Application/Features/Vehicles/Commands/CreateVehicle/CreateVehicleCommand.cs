using Application.ApiContracts.Maintenance.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Vehicles.Commands.CreateVehicle;

public sealed record CreateVehicleCommand : IRequest<Result<VehicleResponse?>>
{
    [JsonPropertyName("lead_id")]
    public int LeadId { get; init; }

    [JsonPropertyName("product_id")]
    public int ProductId { get; init; }

    [JsonPropertyName("vin_number")]
    public string VinNumber { get; init; } = string.Empty;

    [JsonPropertyName("engine_number")]
    public string EngineNumber { get; init; } = string.Empty;

    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; init; }

    [JsonPropertyName("purchase_date")]
    public DateTimeOffset? PurchaseDate { get; init; }
}
