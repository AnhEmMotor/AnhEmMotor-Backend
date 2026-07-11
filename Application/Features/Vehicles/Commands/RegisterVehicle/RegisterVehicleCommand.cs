using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Vehicles.Commands.RegisterVehicle;

public sealed record RegisterVehicleCommand : IRequest<Result<VehicleResponse?>>
{
    [JsonPropertyName("user_id")]
    public Guid? UserId { get; init; }

    [JsonPropertyName("plate")]
    public string? LicensePlate { get; init; }

    [JsonPropertyName("vin_number")]
    public string? VinNumber { get; init; }

    [JsonPropertyName("engine_number")]
    public string? EngineNumber { get; init; }

    [JsonPropertyName("color")]
    public string? Color { get; init; }

    [JsonPropertyName("purchase_date")]
    public DateTimeOffset? PurchaseDate { get; init; }

    [JsonPropertyName("warranty_date")]
    public DateTimeOffset? WarrantyDate { get; init; }

    [JsonPropertyName("current_odometer")]
    public double? CurrentOdo { get; init; }
}
