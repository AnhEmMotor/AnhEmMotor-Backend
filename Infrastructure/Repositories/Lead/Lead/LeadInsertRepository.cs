using Application.Interfaces.Repositories.Lead.Lead;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Lead.Lead;

public class LeadInsertRepository(ApplicationDBContext context) : ILeadInsertRepository
{
    public void Add(Domain.Entities.Lead lead)
    {
        context.Leads.Add(lead);
    }

    public async Task AddAsync(Domain.Entities.Lead lead, CancellationToken cancellationToken = default)
    {
        context.Leads.Add(lead);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
