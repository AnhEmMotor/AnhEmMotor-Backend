using Application.Interfaces.Repositories.Lead.Lead;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Lead.Lead;

public class LeadUpdateRepository(ApplicationDBContext context) : ILeadUpdateRepository
{
    public void Update(Domain.Entities.Lead lead)
    {
        context.Leads.Update(lead);
    }

    public async Task UpdateAsync(Domain.Entities.Lead lead, CancellationToken cancellationToken = default)
    {
        context.Leads.Update(lead);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Restore(Domain.Entities.Lead lead)
    {
        lead.DeletedAt = null;
        context.Leads.Update(lead);
    }
}
