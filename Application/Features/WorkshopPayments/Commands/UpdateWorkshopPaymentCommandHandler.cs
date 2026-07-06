using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WorkshopPayment;
using MediatR;

namespace Application.Features.WorkshopPayments.Commands;

public class UpdateWorkshopPaymentCommandHandler(
    IWorkshopPaymentReadRepository readRepo,
    IWorkshopPaymentWriteRepository writeRepo,
    IUnitOfWork uow) : IRequestHandler<UpdateWorkshopPaymentCommand, Result>
{
    public async Task<Result> Handle(UpdateWorkshopPaymentCommand req, CancellationToken ct)
    {
        var entity = await readRepo.GetByIdAsync(req.Id, ct);
        if (entity is null)
            return Result.Failure([Error.NotFound($"Không tìm thấy phiếu thu id={req.Id}", "Id")]);

        if (req.CustomerName is not null) entity.CustomerName = req.CustomerName;
        if (req.CustomerPhone is not null) entity.CustomerPhone = req.CustomerPhone;
        if (req.VehicleInfo is not null) entity.VehicleInfo = req.VehicleInfo;
        if (req.ServiceDescription is not null) entity.ServiceDescription = req.ServiceDescription;
        entity.SubTotal = req.SubTotal;
        entity.DiscountAmount = req.DiscountAmount;
        entity.TotalAmount = req.TotalAmount;
        entity.PaymentMethod = req.PaymentMethod;
        entity.PaymentStatus = req.PaymentStatus;
        entity.PaidAt = req.PaidAt;
        if (req.Notes is not null) entity.Notes = req.Notes;

        writeRepo.Update(entity);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
