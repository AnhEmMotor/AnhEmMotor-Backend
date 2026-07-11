using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Vehicles.Commands.CreateVehicleMaintenanceHistory;

public sealed record CreateVehicleMaintenanceHistoryCommand : IRequest<Result<int>>
{
    [JsonPropertyName("vehicle_id")]
    public int VehicleId { get; init; }

    [JsonPropertyName("user_id")]
    public Guid? UserId { get; init; }

    [JsonPropertyName("maintenance_date")]
    public DateTimeOffset MaintenanceDate { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("mileage")]
    public int Mileage { get; init; }

    [JsonPropertyName("technician_id")]
    public int? TechnicianId { get; init; }

    [JsonPropertyName("parts_cost")]
    public decimal PartsCost { get; init; }

    [JsonPropertyName("labor_cost")]
    public decimal LaborCost { get; init; }

    [JsonPropertyName("parts_json")]
    public string? PartsJson { get; init; }

    [JsonPropertyName("next_maintenance_date")]
    public DateTimeOffset? NextMaintenanceDate { get; init; }

    [JsonPropertyName("next_maintenance_odo")]
    public int? NextMaintenanceOdo { get; init; }
}
