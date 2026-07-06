using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MaintenanceHistory;
using MediatR;
using System.Text.Json;

namespace Application.Features.RepairOrders.Commands;

public class IssuePartsCommandHandler(
    IMaintenanceHistoryReadRepository readRepo,
    IMaintenanceHistoryWriteRepository writeRepo,
    IUnitOfWork uow) : IRequestHandler<IssuePartsCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(IssuePartsCommand req, CancellationToken ct)
    {
        var history = await readRepo.GetByIdAsync(req.RepairOrderId, ct);
        if (history is null)
            return Result<bool>.Failure([Error.NotFound("Không tìm thấy phiếu sửa chữa.", "RepairOrderId")]);

        var partsCost = req.Parts.Sum(p => p.Price * p.Count);
        var laborCost = req.Services.Sum(s => s.LaborCost);

        history.PartsCost = partsCost;
        history.LaborCost = laborCost;
        history.TotalCost = partsCost + laborCost;

        var items = new
        {
            Parts = req.Parts,
            Services = req.Services
        };
        history.PartsJson = JsonSerializer.Serialize(items);
        
        writeRepo.Update(history);
        await uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
