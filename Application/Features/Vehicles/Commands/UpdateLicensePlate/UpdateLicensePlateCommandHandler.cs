using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Vehicles.Commands.UpdateLicensePlate;

public sealed class UpdateLicensePlateCommandHandler(
    IVehicleReadRepository readRepository,
    IVehicleUpdateRepository updateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateLicensePlateCommand, Result<VehicleResponse?>>
{
    public async Task<Result<VehicleResponse?>> Handle(
        UpdateLicensePlateCommand request,
        CancellationToken cancellationToken)
    {
        var vehicle = await readRepository.GetQuery(DataFetchMode.ActiveOnly)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (vehicle == null)
        {
            return Result<VehicleResponse?>.Failure(Error.NotFound($"Vehicle with ID {request.Id} not found."));
        }
        vehicle.LicensePlate = request.LicensePlate.Trim();
        updateRepository.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return vehicle.Adapt<VehicleResponse>();
    }
}
