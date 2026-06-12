using Application.Interfaces.Repositories.SalesContract;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.SalesContract;

public class SalesContractInsertRepository(
    ApplicationDBContext context) : ISalesContractInsertRepository
{
    public void Add(global::Domain.Entities.SalesContract entity)
    {
        context.SalesContracts.Add(entity);
    }
}
