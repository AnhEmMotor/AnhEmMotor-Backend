using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Mapster;
using MediatR;

namespace Application.Features.Vehicles.Commands.UpdateClientVehicle;

public class UpdateClientVehicleCommandHandler(
    IVehicleReadRepository readRepository,
    IVehicleUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateClientVehicleCommand, Result<VehicleResponse?>>
{
    public async Task<Result<VehicleResponse?>> Handle(UpdateClientVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await readRepository.GetByUserIdAndIdAsync(request.UserId, request.VehicleId, cancellationToken)
            .ConfigureAwait(false);

        if (vehicle is null)
        {
            return Result<VehicleResponse?>.Failure(Error.NotFound("Vehicle not found.", "VehicleId"));
        }

        if (!string.IsNullOrWhiteSpace(request.LicensePlate))
        {
            vehicle.LicensePlate = request.LicensePlate.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Color))
        {
            vehicle.Color = request.Color.Trim();
        }

        if (request.CurrentOdo.HasValue)
        {
            vehicle.CurrentOdo = request.CurrentOdo.Value;
        }

        if (request.WarrantyDate.HasValue)
        {
            vehicle.WarrantyDate = request.WarrantyDate;
        }

        if (!string.IsNullOrWhiteSpace(request.VinNumber))
        {
            vehicle.VinNumber = request.VinNumber.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.EngineNumber))
        {
            vehicle.EngineNumber = request.EngineNumber.Trim();
        }

        updateRepository.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return vehicle.Adapt<VehicleResponse>();
    }
}
