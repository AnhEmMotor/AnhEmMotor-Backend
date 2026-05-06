using Application.Interfaces.Repositories.Lead;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Lead;

public class LeadActivityInsertRepository(ApplicationDBContext context) : ILeadActivityInsertRepository
{
    public void Add(LeadActivity activity)
    {
        context.LeadActivities.Add(activity);
    }
}
