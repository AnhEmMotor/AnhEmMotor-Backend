using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.Vehicle;
using MediatR;

namespace Application.Features.RepairOrders.Queries;

public class GetRepairOrderDetailQueryHandler(
    IMaintenanceHistoryReadRepository repo,
    IVehicleReadRepository vehicleRepo,
    IEmployeeReadRepository employeeRepo) : IRequestHandler<GetRepairOrderDetailQuery, Result<RepairOrderResponse>>
{
    public async Task<Result<RepairOrderResponse>> Handle(GetRepairOrderDetailQuery req, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(req.Id, ct);
        if (entity is null)
            return Result<RepairOrderResponse>.Failure(
                [Error.NotFound($"Không tìm thấy lệnh sửa chữa id={req.Id}", "Id")]);
        var vehicle = await vehicleRepo.GetByIdAsync(entity.VehicleId, ct);
        string? vehicleInfo = vehicle != null
            ? (!string.IsNullOrEmpty(vehicle.LicensePlate) ? vehicle.LicensePlate : vehicle.VinNumber)
            : null;
        string? technicianName = null;
        if (entity.TechnicianId.HasValue)
        {
            var emp = await employeeRepo.GetByIdAsync(entity.TechnicianId.Value, ct);
            technicianName = emp?.User?.FullName;
        }
        return Result<RepairOrderResponse>.Success(
            new RepairOrderResponse
            {
                Id = entity.Id,
                MaintenanceNumber = entity.MaintenanceNumber,
                VehicleId = entity.VehicleId,
                VehicleInfo = vehicleInfo,
                MaintenanceDate = entity.MaintenanceDate,
                Description = entity.Description,
                Mileage = entity.Mileage,
                TechnicianId = entity.TechnicianId,
                TechnicianName = technicianName,
                PartsCost = entity.PartsCost,
                LaborCost = entity.LaborCost,
                TotalCost = entity.TotalCost,
                PartsJson = entity.PartsJson,
                NextMaintenanceDate = entity.NextMaintenanceDate,
                NextMaintenanceOdo = entity.NextMaintenanceOdo,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            });
    }
}
