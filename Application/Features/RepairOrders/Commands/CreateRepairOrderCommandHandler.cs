using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.MaintenanceHistory;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants.Order;
using Domain.Entities;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public class CreateRepairOrderCommandHandler(
    IMaintenanceHistoryWriteRepository writeRepo,
    IVehicleReadRepository vehicleRepo,
    IVehicleUpdateRepository vehicleUpdateRepo,
    ILeadReadRepository leadReadRepo,
    ILeadInsertRepository leadInsertRepo,
    IUnitOfWork uow) : IRequestHandler<CreateRepairOrderCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateRepairOrderCommand req, CancellationToken ct)
    {
        int vehicleId = 0;
        if (req.VehicleId.HasValue && req.VehicleId.Value > 0)
        {
            var vehicle = await vehicleRepo.GetByIdAsync(req.VehicleId.Value, ct);
            if (vehicle is null)
                return Result<int>.Failure([Error.BadRequest("Xe không tồn tại.", "VehicleId")]);
            vehicleId = vehicle.Id;
        } else
        {
            if (string.IsNullOrWhiteSpace(req.CustomerPhone))
                return Result<int>.Failure([Error.BadRequest("Số điện thoại không được để trống.", "CustomerPhone")]);
            var lead = await leadReadRepo.GetByPhoneNumberAsync(req.CustomerPhone, ct);
            if (lead == null)
            {
                lead = new Lead
                {
                    FullName = string.IsNullOrWhiteSpace(req.CustomerName) ? "Khách hàng mới" : req.CustomerName,
                    PhoneNumber = req.CustomerPhone,
                    Notes = "Tạo tự động từ Tạo Phiếu Sửa Chữa"
                };
                await leadInsertRepo.AddAsync(lead, ct);
                await uow.SaveChangesAsync(ct);
            }
            var vehicle = new Vehicle
            {
                LeadId = lead.Id,
                LicensePlate = req.LicensePlate ?? string.Empty,
                VinNumber = req.VinNumber ?? string.Empty,
                Status = VehicleStatus.Available,
                PurchaseDate = DateTimeOffset.UtcNow
            };
            vehicleUpdateRepo.Add(vehicle);
            await uow.SaveChangesAsync(ct);
            vehicleId = vehicle.Id;
        }
        var initialLaborCost = req.LaborCost == 0 ? 200000m : req.LaborCost;
        var totalCost = req.PartsCost + initialLaborCost;
        var dateStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var number = $"RO-{dateStr}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var entity = new MaintenanceHistory
        {
            VehicleId = vehicleId,
            MaintenanceDate = req.MaintenanceDate,
            Description = req.Description,
            Mileage = req.Mileage,
            TechnicianId = req.TechnicianId,
            PartsCost = req.PartsCost,
            LaborCost = initialLaborCost,
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
