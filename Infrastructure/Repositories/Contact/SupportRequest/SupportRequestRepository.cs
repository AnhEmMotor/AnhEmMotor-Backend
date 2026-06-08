using Application.Interfaces.Repositories.Contact;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using DomainEntities = global::Domain.Entities;

namespace Infrastructure.Repositories.Contact.SupportRequest;

public class SupportRequestRepository(ApplicationDBContext context) : ISupportRequestRepository
{
    public Task<DomainEntities.SupportRequest?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.SupportRequests
            .Include(s => s.Contact)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<List<DomainEntities.SupportRequest>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.SupportRequests
            .Include(s => s.Contact)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(DomainEntities.SupportRequest entity, CancellationToken cancellationToken)
    {
        context.SupportRequests.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DomainEntities.SupportRequest entity, CancellationToken cancellationToken)
    {
        context.SupportRequests.Update(entity);
        return Task.CompletedTask;
    }
}
