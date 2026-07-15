using Application.Interfaces.Repositories.FinanceContract;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.FinanceContract;

using FinanceContractEntity = Domain.Entities.FinanceContract;

public class FinanceContractInsertRepository(ApplicationDBContext context) : IFinanceContractInsertRepository
{
    public void Add(FinanceContractEntity entity)
    {
        context.FinanceContracts.Add(entity);
    }
}
