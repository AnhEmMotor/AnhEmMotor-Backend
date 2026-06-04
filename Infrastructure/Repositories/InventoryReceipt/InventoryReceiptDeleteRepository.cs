using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;
using System.Linq;
using System.Collections.Generic;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InventoryReceiptDeleteRepository(ApplicationDBContext context) : IInventoryReceiptDeleteRepository
{
    public void Delete(InventoryReceiptEntity inventoryReceipt)
    {
        context.SoftDeleteUsingSetColumn(inventoryReceipt);
        
        var infos = context.InventoryReceiptInfos.Where(ii => ii.InventoryReceiptId == inventoryReceipt.Id).ToList();
        foreach (var info in infos)
        {
            context.SoftDeleteUsingSetColumn(info);
            var vehicles = context.Vehicles.Where(v => v.InventoryReceiptInfoId == info.Id).ToList();
            context.SoftDeleteUsingSetColumnRange(vehicles);
        }
    }

    public void Delete(IEnumerable<InventoryReceiptEntity> inventoryReceipts)
    {
        var ids = inventoryReceipts.Select(x => x.Id).ToList();
        context.SoftDeleteUsingSetColumnRange(inventoryReceipts);

        var infos = context.InventoryReceiptInfos.Where(ii => ids.Contains(ii.InventoryReceiptId)).ToList();
        foreach (var info in infos)
        {
            context.SoftDeleteUsingSetColumn(info);
            var vehicles = context.Vehicles.Where(v => v.InventoryReceiptInfoId == info.Id).ToList();
            context.SoftDeleteUsingSetColumnRange(vehicles);
        }
    }

    public void DeleteInventoryReceiptInfo(InventoryReceiptInfoEntity inventoryReceiptInfo)
    {
        context.InventoryReceiptInfos.Remove(inventoryReceiptInfo);
    }
}
