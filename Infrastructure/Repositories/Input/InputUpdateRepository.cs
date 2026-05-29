using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using InputEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InputUpdateRepository(ApplicationDBContext context) : IInputUpdateRepository
{
    public void Update(InputEntity InventoryReceipt)
    {
        context.InputReceipts.Update(InventoryReceipt);
    }

    public void Restore(InputEntity InventoryReceipt)
    {
        context.Restore(InventoryReceipt);
    }

    public void Restore(IEnumerable<InputEntity> InventoryReceipts)
    {
        context.RestoreDeleteUsingSetColumnRange(InventoryReceipts);
    }
}
