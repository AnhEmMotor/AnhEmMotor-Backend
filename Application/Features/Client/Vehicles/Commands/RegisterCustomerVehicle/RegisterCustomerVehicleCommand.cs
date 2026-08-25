using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Client.Vehicles.Commands.RegisterCustomerVehicle;

public class RegisterCustomerVehicleCommand : IRequest<Result<VehicleResponse>>
{
    public Guid UserId { get; set; }

    public string LicensePlate { get; set; } = string.Empty;

    public string VinNumber { get; set; } = string.Empty;

    public string EngineNumber { get; set; } = string.Empty;

    public double CurrentOdo { get; set; }
}
