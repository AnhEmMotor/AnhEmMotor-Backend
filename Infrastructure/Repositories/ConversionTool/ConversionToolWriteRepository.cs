using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.ConversionTool;

public class ConversionToolWriteRepository(ApplicationDBContext context) : IConversionToolWriteRepository
{
    private readonly ApplicationDBContext _context = context;

    public async Task<Domain.Entities.ConversionTool> AddAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<Domain.Entities.ConversionTool>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default)
    {
        _context.Set<Domain.Entities.ConversionTool>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default)
    {
        _context.Set<Domain.Entities.ConversionTool>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<Domain.Entities.ConversionTool> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<Domain.Entities.ConversionTool>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
