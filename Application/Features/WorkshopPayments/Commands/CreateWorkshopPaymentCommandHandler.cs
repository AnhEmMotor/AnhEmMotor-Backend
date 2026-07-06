using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Entities;
using MediatR;

namespace Application.Features.WorkshopPayments.Commands;

public class CreateWorkshopPaymentCommandHandler(
    IWorkshopPaymentWriteRepository writeRepo,
    IWorkshopPaymentReadRepository readRepo,
    IUnitOfWork uow) : IRequestHandler<CreateWorkshopPaymentCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateWorkshopPaymentCommand req, CancellationToken ct)
    {
        var dateStr = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var number = $"WP-{dateStr}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var entity = new WorkshopPayment
        {
            PaymentNumber = number,
            SourceType = req.SourceType,
            SourceId = req.SourceId,
            CustomerName = req.CustomerName,
            CustomerPhone = req.CustomerPhone,
            VehicleInfo = req.VehicleInfo,
            ServiceDescription = req.ServiceDescription,
            SubTotal = req.SubTotal,
            DiscountAmount = req.DiscountAmount,
            TotalAmount = req.TotalAmount,
            PaymentMethod = req.PaymentMethod,
            PaymentStatus = req.PaymentStatus,
            PaidAt = req.PaidAt,
            Notes = req.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        writeRepo.Add(entity);
        await uow.SaveChangesAsync(ct);

        return Result<int>.Success(entity.Id);
    }
}
