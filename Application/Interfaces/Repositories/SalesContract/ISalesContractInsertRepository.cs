using Domain.Entities;

namespace Application.Interfaces.Repositories.SalesContract;

public interface ISalesContractInsertRepository
{
    void Add(global::Domain.Entities.SalesContract entity);
}
