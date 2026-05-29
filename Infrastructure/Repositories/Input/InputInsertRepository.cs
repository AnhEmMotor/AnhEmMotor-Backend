using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using InputEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InputInsertRepository(ApplicationDBContext context) : IInputInsertRepository
{
    public void Add(InputEntity InventoryReceipt)
    {
        context.InputReceipts.Add(InventoryReceipt);
    }
}
