using Application.Interfaces.Repositories.Lead;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Lead;

public class LeadReadRepository(ApplicationDBContext context) : ILeadReadRepository
{
    public Task<Domain.Entities.Lead?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.Leads.Include(l => l.Activities).FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public Task<Domain.Entities.Lead?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        return context.Leads
            .Include(l => l.Activities)
            .FirstOrDefaultAsync(l => string.Compare(l.PhoneNumber, phoneNumber) == 0, cancellationToken);
    }

    public Task<List<Domain.Entities.Lead>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return context.Leads.Include(l => l.Activities).OrderByDescending(l => l.Score).ToListAsync(cancellationToken);
    }

    public IQueryable<Domain.Entities.Lead> GetQueryable()
    {
        return context.Leads.AsQueryable();
    }
}
