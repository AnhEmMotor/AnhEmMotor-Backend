using Application.Interfaces.Repositories.Contact;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using DomainEntities = Domain.Entities;

namespace Infrastructure.Repositories.Contact.CustomerFeedback;

public class CustomerFeedbackRepository(ApplicationDBContext context) : ICustomerFeedbackRepository
{
    public Task<DomainEntities.CustomerFeedback?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.CustomerFeedbacks.Include(f => f.Contact).FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public Task<List<DomainEntities.CustomerFeedback>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.CustomerFeedbacks
            .Include(f => f.Contact)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(DomainEntities.CustomerFeedback entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.CustomerFeedbacks.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DomainEntities.CustomerFeedback entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.CustomerFeedbacks.Update(entity);
        return Task.CompletedTask;
    }
}
