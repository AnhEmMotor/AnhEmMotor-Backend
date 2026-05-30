using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.User;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.Lead.Lead;

public class LeadUpdateRepository(ApplicationDBContext context, IUserReadRepository userReadRepository) : ILeadUpdateRepository
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

    public async Task AssignLeadAsync(Domain.Entities.Lead lead, CancellationToken cancellationToken = default)
    {
        var sales = await userReadRepository.GetUsersInRoleAsync("Sale", cancellationToken).ConfigureAwait(false);
        if (!sales.Any())
            return;
        var salesWithCounts = await context.Users
            .Where(u => sales.Select(s => s.Id).Contains(u.Id))
            .Select(
                u => new
                {
                    User = u,
                    LeadCount = context.Leads.Count(l => l.AssignedToId == u.Id && l.Status != "Closed")
                })
            .OrderBy(x => x.LeadCount)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (salesWithCounts != null)
        {
            lead.AssignedToId = salesWithCounts.User.Id;
        }
    }
}
