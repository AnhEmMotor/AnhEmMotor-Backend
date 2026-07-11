using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Vehicles.Commands.CreateVehicleWarrantyHistory;

public sealed record CreateVehicleWarrantyHistoryCommand : IRequest<Result<int>>
{
    [JsonPropertyName("vehicle_id")]
    public int VehicleId { get; init; }

    [JsonPropertyName("user_id")]
    public Guid? UserId { get; init; }

    [JsonPropertyName("start_date")]
    public DateTimeOffset StartDate { get; init; }

    [JsonPropertyName("end_date")]
    public DateTimeOffset? EndDate { get; init; }

    [JsonPropertyName("provider_name")]
    public string ProviderName { get; init; } = string.Empty;

    [JsonPropertyName("policy_number")]
    public string PolicyNumber { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("coverage_amount")]
    public decimal CoverageAmount { get; init; }
}
