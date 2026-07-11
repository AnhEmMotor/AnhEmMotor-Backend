using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;

namespace Application.Features.Vehicles.Commands.DeleteClientVehicle;

public class DeleteClientVehicleCommandHandler(
    IVehicleReadRepository readRepository,
    IVehicleUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteClientVehicleCommand, Result>
{
    public async Task<Result> Handle(DeleteClientVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await readRepository.GetByUserIdAndIdAsync(request.UserId, request.VehicleId, cancellationToken)
            .ConfigureAwait(false);

        if (vehicle is null)
        {
            return Result.Failure(Error.NotFound("Vehicle not found.", "VehicleId"));
        }

        vehicle.IsActive = false;
        updateRepository.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
