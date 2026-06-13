using Domain.Entities;

namespace Application.Interfaces.Repositories.Contact;

public interface IJobApplicationRepository
{
    public Task<JobApplication?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<List<JobApplication>> GetAllAsync(CancellationToken cancellationToken);

    public Task AddAsync(JobApplication entity, CancellationToken cancellationToken);

    public Task UpdateAsync(JobApplication entity, CancellationToken cancellationToken);
}
