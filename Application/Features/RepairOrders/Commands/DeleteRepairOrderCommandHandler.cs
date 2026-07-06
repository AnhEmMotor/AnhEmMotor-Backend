using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MaintenanceHistory;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public class DeleteRepairOrderCommandHandler(
    IMaintenanceHistoryReadRepository readRepo,
    IMaintenanceHistoryWriteRepository writeRepo,
    IUnitOfWork uow) : IRequestHandler<DeleteRepairOrderCommand, Result>
{
    public async Task<Result> Handle(DeleteRepairOrderCommand req, CancellationToken ct)
    {
        var entity = await readRepo.GetByIdAsync(req.Id, ct, DataFetchMode.ActiveOnly);
        if (entity is null)
            return Result.Failure([Error.NotFound($"Không tìm thấy lệnh sửa chữa id={req.Id}", "Id")]);

        writeRepo.Delete(entity);
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
