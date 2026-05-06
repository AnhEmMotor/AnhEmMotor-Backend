using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Services;

public class LeadAssignmentService(IUserReadRepository userReadRepository, ApplicationDBContext context) : ILeadAssignmentService
{
    public async Task AssignLeadAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        var sales = await userReadRepository.GetUsersInRoleAsync("Sale", cancellationToken);
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
            .FirstOrDefaultAsync(cancellationToken);
        if (salesWithCounts != null)
        {
            lead.AssignedToId = salesWithCounts.User.Id;
        }
    }
}
