using InputEntity = Domain.Entities.InventoryReceipt;
using InputInfoEntity = Domain.Entities.InputInfo;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInputDeleteRepository
{
    public void Delete(InputEntity InventoryReceipt);

    public void Delete(IEnumerable<InputEntity> InventoryReceipts);

    public void DeleteInputInfo(InputInfoEntity inputInfo);
}
