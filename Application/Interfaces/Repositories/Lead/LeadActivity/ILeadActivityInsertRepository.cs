using LeadActivityEntity = Domain.Entities.LeadActivity;

namespace Application.Interfaces.Repositories.Lead.LeadActivity;

public interface ILeadActivityInsertRepository
{
    public void Add(LeadActivityEntity activity);
}
