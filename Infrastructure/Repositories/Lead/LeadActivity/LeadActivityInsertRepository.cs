using Application.Interfaces.Repositories.Lead.LeadActivity;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Lead.LeadActivity;

public class LeadActivityInsertRepository(ApplicationDBContext context) : ILeadActivityInsertRepository
{
    public void Add(Domain.Entities.LeadActivity activity)
    {
        context.LeadActivities.Add(activity);
    }
}
