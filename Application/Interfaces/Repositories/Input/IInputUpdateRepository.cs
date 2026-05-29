using InputEntity = Domain.Entities.InventoryReceipt;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInputUpdateRepository
{
    public void Update(InputEntity InventoryReceipt);

    public void Restore(InputEntity InventoryReceipt);

    public void Restore(IEnumerable<InputEntity> InventoryReceipts);
}
