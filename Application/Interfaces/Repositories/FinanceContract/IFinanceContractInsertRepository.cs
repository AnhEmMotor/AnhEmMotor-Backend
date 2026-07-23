using FinanceContractEntity = Domain.Entities.FinanceContract;

namespace Application.Interfaces.Repositories.FinanceContract;

public interface IFinanceContractInsertRepository
{
    public void Add(FinanceContractEntity entity);
}
