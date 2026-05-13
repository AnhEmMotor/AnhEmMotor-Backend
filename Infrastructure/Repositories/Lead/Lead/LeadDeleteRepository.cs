using Application.Interfaces.Repositories.Lead.Lead;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Lead.Lead;

public class LeadDeleteRepository(ApplicationDBContext context) : ILeadDeleteRepository
{
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        var activities = await context.LeadActivities.ToListAsync(cancellationToken).ConfigureAwait(false);
        context.LeadActivities.RemoveRange(activities);
        var leads = await context.Leads.ToListAsync(cancellationToken).ConfigureAwait(false);
        context.Leads.RemoveRange(leads);
    }
}
