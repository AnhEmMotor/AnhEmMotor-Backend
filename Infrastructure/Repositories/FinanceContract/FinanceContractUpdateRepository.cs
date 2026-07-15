using Application.Interfaces.Repositories.FinanceContract;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.FinanceContract;

using FinanceContractEntity = Domain.Entities.FinanceContract;

public class FinanceContractUpdateRepository(ApplicationDBContext context) : IFinanceContractUpdateRepository
{
    public void Update(FinanceContractEntity entity)
    {
        context.FinanceContracts.Update(entity);
    }
}
