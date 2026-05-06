using Application.Interfaces.Repositories.Lead;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

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
        // Remove all activities first
        var activities = await context.LeadActivities.ToListAsync(cancellationToken);
        context.LeadActivities.RemoveRange(activities);

        // Remove all leads
        var leads = await context.Leads.ToListAsync(cancellationToken);
        context.Leads.RemoveRange(leads);
    }
}
