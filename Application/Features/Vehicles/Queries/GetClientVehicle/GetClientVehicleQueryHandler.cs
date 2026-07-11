using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Vehicle;
using Mapster;
using MediatR;

namespace Application.Features.Vehicles.Queries.GetClientVehicle;

public class GetClientVehicleQueryHandler(IVehicleReadRepository vehicleReadRepository)
    : IRequestHandler<GetClientVehicleQuery, Result<VehicleResponse?>>
{
    public async Task<Result<VehicleResponse?>> Handle(GetClientVehicleQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleReadRepository.GetByUserIdAndIdAsync(request.UserId, request.VehicleId, cancellationToken)
            .ConfigureAwait(false);

        if (vehicle is null)
        {
            return Result<VehicleResponse?>.Failure(Error.NotFound("Vehicle not found.", "VehicleId"));
        }

        return vehicle.Adapt<VehicleResponse>();
    }
}
