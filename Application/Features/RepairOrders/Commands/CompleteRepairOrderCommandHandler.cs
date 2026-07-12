using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Entities;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public class CompleteRepairOrderCommandHandler(
    IMaintenanceHistoryReadRepository readRepo,
    IWorkshopPaymentWriteRepository paymentWriteRepo,
    IUnitOfWork uow) : IRequestHandler<CompleteRepairOrderCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CompleteRepairOrderCommand req, CancellationToken ct)
    {
        var history = await readRepo.GetByIdAsync(req.RepairOrderId, ct);
        if (history is null)
            return Result<bool>.Failure([Error.NotFound("Không tìm thấy phiếu sửa chữa.", "RepairOrderId")]);
        var dateStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var paymentNumber = $"PMT-{dateStr}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var payment = new WorkshopPayment
        {
            SourceType = "Maintenance",
            SourceId = history.Id,
            PaymentNumber = paymentNumber,
            SubTotal = history.TotalCost,
            TotalAmount = history.TotalCost,
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
