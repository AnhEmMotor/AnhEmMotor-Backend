using Application.Interfaces.Repositories.FinanceContract;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.FinanceContract;

using FinanceContractEntity = Domain.Entities.FinanceContract;

public class FinanceContractDeleteRepository(ApplicationDBContext context) : IFinanceContractDeleteRepository
{
    public void Delete(FinanceContractEntity entity)
    {
        context.FinanceContracts.Remove(entity);
    }

    public void Delete(IEnumerable<FinanceContractEntity> entities)
    {
        context.FinanceContracts.RemoveRange(entities);
    }
}
