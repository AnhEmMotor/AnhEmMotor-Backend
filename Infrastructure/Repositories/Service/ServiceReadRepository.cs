using Application.Interfaces.Repositories.Service;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using ServiceEntity = Domain.Entities.Service;

namespace Infrastructure.Repositories.Service;

public class ServiceReadRepository(ApplicationDBContext context) : IServiceReadRepository
{
    public IQueryable<ServiceEntity> GetQueryable()
    {
        return context.Services.AsNoTracking();
    }

    public async Task<IEnumerable<ServiceEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.Services
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ServiceEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return context.Services
            .AnyAsync(s => s.Id == id, cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return context.Services
            .AnyAsync(s => s.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public Task<bool> NameExistsExcludingAsync(
        string name,
        int excludeId,
        CancellationToken cancellationToken = default)
    {
        return context.Services
            .AnyAsync(s => s.Id != excludeId && s.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}
