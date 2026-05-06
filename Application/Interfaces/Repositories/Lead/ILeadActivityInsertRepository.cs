using Domain.Entities;

namespace Application.Interfaces.Repositories.Lead;

public interface ILeadActivityInsertRepository
{
    public void Add(LeadActivity activity);
}
