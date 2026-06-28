using Application.Interfaces.Repositories.Contact;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using DomainEntities = Domain.Entities;

namespace Infrastructure.Repositories.Contact.JobApplication;

public class JobApplicationRepository(ApplicationDBContext context) : IJobApplicationRepository
{
    public Task<DomainEntities.JobApplication?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.JobApplications
            .Include(j => j.Contact)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.RepliedBy)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public Task<List<DomainEntities.JobApplication>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.JobApplications
            .Include(j => j.Contact)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.RepliedBy)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(DomainEntities.JobApplication entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.JobApplications.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DomainEntities.JobApplication entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        context.JobApplications.Update(entity);
        return Task.CompletedTask;
    }
}
