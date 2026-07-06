using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Constants;
using MediatR;

namespace Application.Features.WorkshopPayments.Commands;

public class DeleteWorkshopPaymentCommandHandler(
    IWorkshopPaymentReadRepository readRepo,
    IWorkshopPaymentWriteRepository writeRepo,
    IUnitOfWork uow) : IRequestHandler<DeleteWorkshopPaymentCommand, Result>
{
    public async Task<Result> Handle(DeleteWorkshopPaymentCommand req, CancellationToken ct)
    {
        var entity = await readRepo.GetByIdAsync(req.Id, ct);
        if (entity is null)
            return Result.Failure([Error.NotFound($"Không tìm thấy phiếu thu id={req.Id}", "Id")]);

        writeRepo.Delete(entity);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
