using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Entities;
using MediatR;
using System.Linq;

namespace Application.Features.RepairOrders.Commands;

public class CompleteRepairOrderCommandHandler(
    IMaintenanceHistoryReadRepository readRepo,
    IVehicleReadRepository vehicleReadRepo,
    IWorkshopPaymentWriteRepository paymentWriteRepo,
    IUnitOfWork uow) : IRequestHandler<CompleteRepairOrderCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CompleteRepairOrderCommand req, CancellationToken ct)
    {
        var history = await readRepo.GetByIdAsync(req.RepairOrderId, ct);
        if (history is null)
            return Result<bool>.Failure([Error.NotFound("Không tìm thấy phiếu sửa chữa.", "RepairOrderId")]);
            
        var vehicles = await vehicleReadRepo.GetByIdsWithLeadAsync(new[] { history.VehicleId }, ct);
        var vehicle = vehicles.FirstOrDefault();
        var customerName = vehicle?.Lead?.FullName ?? vehicle?.User?.FullName ?? "Khách hàng";
        var customerPhone = vehicle?.Lead?.PhoneNumber ?? vehicle?.User?.PhoneNumber ?? "";
        var vehicleInfo = !string.IsNullOrEmpty(vehicle?.LicensePlate) ? vehicle.LicensePlate : (vehicle?.VinNumber ?? "");

        var dateStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var paymentNumber = $"PMT-{dateStr}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var payment = new WorkshopPayment
        {
            SourceType = "Maintenance",
            SourceId = history.Id,
            PaymentNumber = paymentNumber,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            VehicleInfo = vehicleInfo,
            ServiceDescription = history.Description,
            SubTotal = history.TotalCost,
            DiscountAmount = req.DiscountAmount ?? 0,
            TotalAmount = history.TotalCost - (req.DiscountAmount ?? 0),
            PaymentMethod = req.PaymentMethod,
            PaymentStatus = req.PaymentStatus,
            Notes = req.Notes,
            PaidAt = req.PaymentStatus == "Paid" ? DateTimeOffset.UtcNow : null
        };
        paymentWriteRepo.Add(payment);
        await uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
