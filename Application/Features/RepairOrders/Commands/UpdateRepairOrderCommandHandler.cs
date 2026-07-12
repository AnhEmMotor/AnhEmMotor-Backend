using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MaintenanceHistory;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public class UpdateRepairOrderCommandHandler(
    IMaintenanceHistoryReadRepository readRepo,
    IMaintenanceHistoryWriteRepository writeRepo,
    IUnitOfWork uow) : IRequestHandler<UpdateRepairOrderCommand, Result>
{
    public async Task<Result> Handle(UpdateRepairOrderCommand req, CancellationToken ct)
    {
        var entity = await readRepo.GetByIdAsync(req.Id, ct);
        if (entity is null)
            return Result.Failure([Error.NotFound($"Không tìm thấy lệnh sửa chữa id={req.Id}", "Id")]);
        entity.VehicleId = req.VehicleId;
        entity.MaintenanceDate = req.MaintenanceDate;
        entity.Description = req.Description;
        entity.Mileage = req.Mileage;
        entity.TechnicianId = req.TechnicianId;
        entity.PartsCost = req.PartsCost;
        entity.LaborCost = req.LaborCost;
        entity.TotalCost = req.PartsCost + req.LaborCost;
        entity.PartsJson = req.PartsJson;
        entity.NextMaintenanceDate = req.NextMaintenanceDate;
        entity.NextMaintenanceOdo = req.NextMaintenanceOdo;
        writeRepo.Update(entity);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
