using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using InputEntity = Domain.Entities.InventoryReceipt;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InputDeleteRepository(ApplicationDBContext context) : IInputDeleteRepository
{
    public void Delete(InputEntity InventoryReceipt)
    {
        context.SoftDeleteUsingSetColumn(InventoryReceipt);
    }

    public void Delete(IEnumerable<InputEntity> InventoryReceipts)
    {
        context.SoftDeleteUsingSetColumnRange(InventoryReceipts);
    }

    public void DeleteInputInfo(InputInfoEntity inputInfo)
    {
        context.InputInfos.Remove(inputInfo);
    }
}
