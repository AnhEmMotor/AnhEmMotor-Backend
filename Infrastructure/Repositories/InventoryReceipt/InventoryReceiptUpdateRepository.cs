using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InventoryReceiptUpdateRepository(ApplicationDBContext context) : IInventoryReceiptUpdateRepository
{
    public void Update(InventoryReceiptEntity InventoryReceipt)
    {
        context.InventoryReceipts.Update(InventoryReceipt);
    }

    public void Restore(InventoryReceiptEntity InventoryReceipt)
    {
        context.Restore(InventoryReceipt);
        var infos = context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Where(ii => ii.InventoryReceiptId == InventoryReceipt.Id && ii.DeletedAt != null)
            .ToList();
        foreach (var info in infos)
        {
            context.Restore(info);
            var vehicles = context.Vehicles
                .IgnoreQueryFilters()
                .Where(v => v.InventoryReceiptInfoId == info.Id && v.DeletedAt != null)
                .ToList();
            context.RestoreDeleteUsingSetColumnRange(vehicles);
        }
    }

    public void Restore(IEnumerable<InventoryReceiptEntity> InventoryReceipts)
    {
        context.RestoreDeleteUsingSetColumnRange(InventoryReceipts);
        var ids = InventoryReceipts.Select(x => x.Id).ToList();
        var infos = context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Where(ii => ids.Contains(ii.InventoryReceiptId) && ii.DeletedAt != null)
            .ToList();
        foreach (var info in infos)
        {
            context.Restore(info);
            var vehicles = context.Vehicles
                .IgnoreQueryFilters()
                .Where(v => v.InventoryReceiptInfoId == info.Id && v.DeletedAt != null)
                .ToList();
            context.RestoreDeleteUsingSetColumnRange(vehicles);
        }
    }
}
