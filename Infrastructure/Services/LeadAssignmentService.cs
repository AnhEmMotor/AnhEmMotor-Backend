using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class LeadAssignmentService(IUserReadRepository userReadRepository, ApplicationDBContext context) : ILeadAssignmentService
{
    public async Task AssignLeadAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        // Get all users with 'Sale' role
        var sales = await userReadRepository.GetUsersInRoleAsync("Sale", cancellationToken);
        
        if (!sales.Any()) return;

        // Round Robin Logic: Find the sale person with the least assigned leads
        // Or just the one who was assigned a lead longest ago.
        // For simplicity: Find user with fewest active leads
        var salesWithCounts = await context.Users
            .Where(u => sales.Select(s => s.Id).Contains(u.Id))
            .Select(u => new
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
