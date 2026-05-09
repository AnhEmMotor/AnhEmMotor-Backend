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
        return context.Leads
            .Include(l => l.Activities)
            .OrderByDescending(l => l.Score)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Domain.Entities.Lead>> GetAllLeadsWithActivitiesAsync(
        CancellationToken cancellationToken = default)
    {
        return context.Leads
            .Include(l => l.Activities)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Domain.Entities.Lead>> GetLoyaltyMembersAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = context.Leads.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l => l.FullName.Contains(search) || l.PhoneNumber.Contains(search));
        }
        return query
            .OrderByDescending(l => l.Points)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.Lead?> GetByIdentificationNumberAsync(
        string identificationNumber,
        CancellationToken cancellationToken = default)
    {
        return context.Leads
            .FirstOrDefaultAsync(l => string.Compare(l.IdentificationNumber, identificationNumber) == 0, cancellationToken);
    }

    public IQueryable<Domain.Entities.Lead> GetQueryable()
    {
        return context.Leads.AsQueryable();
    }
}
