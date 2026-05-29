using InputEntity = Domain.Entities.InventoryReceipt;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInputInsertRepository
{
    public void Add(InputEntity InventoryReceipt);
}
