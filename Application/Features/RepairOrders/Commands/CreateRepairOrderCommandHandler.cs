using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MaintenanceHistory;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public class CreateRepairOrderCommandHandler(
    IMaintenanceHistoryWriteRepository writeRepo,
    IMaintenanceHistoryReadRepository readRepo,
    IVehicleReadRepository vehicleRepo,
    IUnitOfWork uow) : IRequestHandler<CreateRepairOrderCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateRepairOrderCommand req, CancellationToken ct)
    {
        var vehicle = await vehicleRepo.GetByIdAsync(req.VehicleId, ct);
        if (vehicle is null)
            return Result<int>.Failure([Error.BadRequest("Xe không tồn tại.", "VehicleId")]);

        var totalCost = req.PartsCost + req.LaborCost;
        var dateStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var number = $"RO-{dateStr}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var entity = new MaintenanceHistory
        {
            VehicleId = req.VehicleId,
            MaintenanceDate = req.MaintenanceDate,
            Description = req.Description,
            Mileage = req.Mileage,
            TechnicianId = req.TechnicianId,
            PartsCost = req.PartsCost,
            LaborCost = req.LaborCost,
            TotalCost = totalCost,
            PartsJson = req.PartsJson,
            NextMaintenanceDate = req.NextMaintenanceDate,
            NextMaintenanceOdo = req.NextMaintenanceOdo,
            MaintenanceNumber = number,
            CreatedAt = DateTimeOffset.UtcNow
        };

        writeRepo.Add(entity);
        await uow.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}
