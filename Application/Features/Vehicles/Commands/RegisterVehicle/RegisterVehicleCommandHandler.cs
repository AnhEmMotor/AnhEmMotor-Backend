using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants.Order;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Vehicles.Commands.RegisterVehicle;

public class RegisterVehicleCommandHandler(
    IVehicleReadRepository readRepository,
    IVehicleUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterVehicleCommand, Result<VehicleResponse?>>
{
    public async Task<Result<VehicleResponse?>> Handle(RegisterVehicleCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId is null)
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("UserId is required.", "UserId"));
        }

        if (string.IsNullOrWhiteSpace(request.VinNumber))
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("VIN cannot be empty.", "VinNumber"));
        }

        if (string.IsNullOrWhiteSpace(request.EngineNumber))
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("Engine number cannot be empty.", "EngineNumber"));
        }

        var existingVehicles = await readRepository.GetByUserIdAsync(request.UserId.Value.ToString(), cancellationToken)
            .ConfigureAwait(false);
        if (existingVehicles.Any(v => string.Equals(v.VinNumber, request.VinNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Result<VehicleResponse?>.Failure(Error.BadRequest("VIN already exists.", "VinNumber"));
        }

        var vehicle = new Vehicle
        {
            UserId = request.UserId,
            LicensePlate = request.LicensePlate?.Trim() ?? string.Empty,
            VinNumber = request.VinNumber.Trim(),
            EngineNumber = request.EngineNumber.Trim(),
            Color = request.Color?.Trim() ?? string.Empty,
            PurchaseDate = request.PurchaseDate ?? DateTimeOffset.UtcNow,
            WarrantyDate = request.WarrantyDate,
            CurrentOdo = request.CurrentOdo ?? 0,
            IsActive = true,
            Status = VehicleStatus.Available
        };

        updateRepository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return vehicle.Adapt<VehicleResponse>();
    }
}
