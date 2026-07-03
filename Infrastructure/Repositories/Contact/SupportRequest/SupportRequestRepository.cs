using Application.Interfaces.Repositories.Contact;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using DomainEntities = Domain.Entities;

namespace Infrastructure.Repositories.Contact.SupportRequest;

public class SupportRequestRepository(ApplicationDBContext context) : ISupportRequestRepository
{
    public Task<DomainEntities.SupportRequest?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.SupportRequests
            .Include(s => s.Contact)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.RepliedBy)
            .Include(s => s.AssignedUser)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<List<DomainEntities.SupportRequest>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.SupportRequests
            .Include(s => s.Contact)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.RepliedBy)
            .Include(s => s.AssignedUser)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(DomainEntities.SupportRequest entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.SupportRequests.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DomainEntities.SupportRequest entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.SupportRequests.Update(entity);
        return Task.CompletedTask;
    }
}
