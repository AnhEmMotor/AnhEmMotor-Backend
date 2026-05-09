using Application.ApiContracts.Maintenance.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Vehicles.Commands.UpdateLicensePlate;

public sealed record UpdateLicensePlateCommand : IRequest<Result<VehicleResponse?>>
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonPropertyName("license_plate")]
    public string LicensePlate { get; init; } = string.Empty;
}
