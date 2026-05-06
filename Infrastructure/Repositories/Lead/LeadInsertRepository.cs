using Application.Interfaces.Repositories.Lead;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Lead;

public class LeadInsertRepository(ApplicationDBContext context) : ILeadInsertRepository
{
    public void Add(Domain.Entities.Lead lead)
    {
        context.Leads.Add(lead);
    }

    public void Update(Domain.Entities.Lead lead)
    {
        context.Leads.Update(lead);
    }
}
