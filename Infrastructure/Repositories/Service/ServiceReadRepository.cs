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

    public Task<IEnumerable<ServiceEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return context.Services
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IEnumerable<ServiceEntity>)t.Result, cancellationToken);
    }

    public Task<ServiceEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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
            .AnyAsync(s => string.Compare(s.Name.ToLower(), name.ToLower()) == 0, cancellationToken);
    }

    public Task<bool> NameExistsExcludingAsync(
        string name,
        int excludeId,
        CancellationToken cancellationToken = default)
    {
        return context.Services
            .AnyAsync(s => s.Id != excludeId && string.Compare(s.Name.ToLower(), name.ToLower()) == 0, cancellationToken);
    }
}
