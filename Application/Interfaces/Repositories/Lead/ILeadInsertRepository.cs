using Domain.Entities;

namespace Application.Interfaces.Repositories.Lead;

public interface ILeadInsertRepository
{
    void Add(Domain.Entities.Lead lead);
    void Update(Domain.Entities.Lead lead);
}
