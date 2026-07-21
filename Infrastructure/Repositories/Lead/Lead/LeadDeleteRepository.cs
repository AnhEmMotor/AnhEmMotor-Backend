using Application.Interfaces.Repositories.Lead.Lead;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var voucherLeads = await context.VoucherLeads
            .Where(vl => vl.LeadId == id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        context.VoucherLeads.RemoveRange(voucherLeads);

        var activities = await context.LeadActivities
            .Where(la => la.LeadId == id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        context.LeadActivities.RemoveRange(activities);

        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (lead != null)
        {
            context.Leads.Remove(lead);
        }
    }
}
