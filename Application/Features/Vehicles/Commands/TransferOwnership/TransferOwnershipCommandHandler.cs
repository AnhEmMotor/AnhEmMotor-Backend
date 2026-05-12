using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Vehicle;
using Mapster;
using MediatR;

namespace Application.Features.Vehicles.Commands.TransferOwnership;

public sealed class TransferOwnershipCommandHandler(
    IVehicleReadRepository readRepository,
    IVehicleUpdateRepository updateRepository,
    ILeadReadRepository leadReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<TransferOwnershipCommand, Result<VehicleResponse?>>
{
    public async Task<Result<VehicleResponse?>> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        var leadExists = await leadReadRepository.ExistsAsync(request.NewLeadId, cancellationToken)
            .ConfigureAwait(false);
        if (!leadExists)
        {
            return Result<VehicleResponse?>.Failure(
                Error.NotFound($"Lead with ID {request.NewLeadId} not found.", "NewLeadId"));
        }
        var vehicle = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (vehicle == null)
        {
            return Result<VehicleResponse?>.Failure(Error.NotFound($"Vehicle with ID {request.Id} not found."));
        }
        vehicle.LeadId = request.NewLeadId;
        updateRepository.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return vehicle.Adapt<VehicleResponse>();
    }
}
