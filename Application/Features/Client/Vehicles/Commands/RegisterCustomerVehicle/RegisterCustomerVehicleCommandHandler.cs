using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using MediatR;

namespace Application.Features.Client.Vehicles.Commands.RegisterCustomerVehicle;

public class RegisterCustomerVehicleCommandHandler(
    IVehicleReadRepository vehicleReadRepository,
    IVehicleUpdateRepository vehicleUpdateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterCustomerVehicleCommand, Result<VehicleResponse>>
{
    public async Task<Result<VehicleResponse>> Handle(
        RegisterCustomerVehicleCommand request,
        CancellationToken cancellationToken)
    {
        var licensePlate = request.LicensePlate.Trim();
        var vinNumber = request.VinNumber.Trim();
        var engineNumber = request.EngineNumber.Trim();

        if (licensePlate.Length == 0)
        {
            return Result<VehicleResponse>.Failure(
                Error.Validation("Biển số xe là bắt buộc.", nameof(request.LicensePlate)));
        }

        var existingPlate = await vehicleReadRepository
            .GetByLicensePlateAsync(licensePlate, cancellationToken)
            .ConfigureAwait(false);
        if (existingPlate is not null && existingPlate.UserId == request.UserId && existingPlate.IsActive)
        {
            return Result<VehicleResponse>.Failure(
                Error.Conflict("Biển số xe này đã tồn tại trong nhà xe của bạn."));
        }

        if (vinNumber.Length > 0)
        {
            var existingVin = await vehicleReadRepository
                .GetByVinAsync(vinNumber, cancellationToken)
                .ConfigureAwait(false);
            if (existingVin is not null && existingVin.UserId == request.UserId && existingVin.IsActive)
            {
                return Result<VehicleResponse>.Failure(
                    Error.Conflict("Số khung (VIN) này đã được đăng ký trong nhà xe của bạn."));
            }
        }

        var vehicle = new Vehicle
        {
            UserId = request.UserId,
            LicensePlate = licensePlate,
            VinNumber = vinNumber,
            EngineNumber = engineNumber,
            CurrentOdo = request.CurrentOdo,
            PurchaseDate = DateTimeOffset.UtcNow,
        };

        vehicleUpdateRepository.Add(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<VehicleResponse>.Success(new VehicleResponse
        {
            Id = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            VinNumber = vehicle.VinNumber,
            EngineNumber = vehicle.EngineNumber,
            PurchaseDate = vehicle.PurchaseDate,
            IsActive = vehicle.IsActive,
        });
    }
}
