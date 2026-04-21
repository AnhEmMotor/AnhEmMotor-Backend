using Domain.Entities;

namespace Application.Interfaces.Repositories.Lead;

public interface ILeadActivityInsertRepository
{
    void Add(LeadActivity activity);
}
