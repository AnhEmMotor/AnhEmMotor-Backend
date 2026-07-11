using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Vehicles.Commands.UpdateClientVehicle;

public sealed record UpdateClientVehicleCommand : IRequest<Result<VehicleResponse?>>
{
    public int VehicleId { get; init; }

    public Guid UserId { get; init; }

    public string? LicensePlate { get; init; }

    public string? Color { get; init; }

    public double? CurrentOdo { get; init; }

    public DateTimeOffset? WarrantyDate { get; init; }

    public string? VinNumber { get; init; }

    public string? EngineNumber { get; init; }
}
