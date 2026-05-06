using Application.Interfaces.Repositories.Lead;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Lead;

public class LeadWriteRepository(ApplicationDBContext context) : ILeadWriteRepository
{
    public void Add(Domain.Entities.Lead lead)
    {
        context.Leads.Add(lead);
    }

    public void Update(Domain.Entities.Lead lead)
    {
        context.Leads.Update(lead);
    }

    public async Task UpdateAsync(Domain.Entities.Lead lead, CancellationToken cancellationToken = default)
    {
        context.Leads.Update(lead);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        var activities = await context.LeadActivities.ToListAsync(cancellationToken);
        context.LeadActivities.RemoveRange(activities);
        var leads = await context.Leads.ToListAsync(cancellationToken);
        context.Leads.RemoveRange(leads);
    }
}
