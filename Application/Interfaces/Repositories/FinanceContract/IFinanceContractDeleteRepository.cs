using FinanceContractEntity = Domain.Entities.FinanceContract;

namespace Application.Interfaces.Repositories.FinanceContract;

public interface IFinanceContractDeleteRepository
{
    public void Delete(FinanceContractEntity entity);

    public void Delete(IEnumerable<FinanceContractEntity> entities);
}
