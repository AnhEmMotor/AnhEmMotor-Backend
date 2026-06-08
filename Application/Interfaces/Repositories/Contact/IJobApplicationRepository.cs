using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<JobApplication>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(JobApplication entity, CancellationToken cancellationToken);
    Task UpdateAsync(JobApplication entity, CancellationToken cancellationToken);
}
