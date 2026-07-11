using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MaintenanceHistory;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;
using Domain.Entities;

namespace Application.Features.Vehicles.Commands.CreateVehicleMaintenanceHistory;

public class CreateVehicleMaintenanceHistoryCommandHandler(
    IVehicleReadRepository vehicleReadRepository,
    IMaintenanceHistoryWriteRepository writeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateVehicleMaintenanceHistoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateVehicleMaintenanceHistoryCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleReadRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result<int>.Failure([Error.NotFound("Vehicle not found.", "VehicleId")]);
        }

        var maintenanceNumber = $"MH-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        var totalCost = request.PartsCost + request.LaborCost;

        var entity = new MaintenanceHistory
        {
            VehicleId = vehicle.Id,
            MaintenanceNumber = maintenanceNumber,
            MaintenanceDate = request.MaintenanceDate,
            Description = request.Description,
            Mileage = request.Mileage,
            TechnicianId = request.TechnicianId,
            PartsCost = request.PartsCost,
            LaborCost = request.LaborCost,
            TotalCost = totalCost,
            PartsJson = request.PartsJson,
            NextMaintenanceDate = request.NextMaintenanceDate,
            NextMaintenanceOdo = request.NextMaintenanceOdo,
            CreatedAt = DateTimeOffset.UtcNow
        };

        writeRepository.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<int>.Success(entity.Id);
    }
}
