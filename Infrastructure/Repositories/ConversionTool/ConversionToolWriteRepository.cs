using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.ConversionTool;

public class ConversionToolWriteRepository(MySqlDbContext context) : IConversionToolWriteRepository
{
    private readonly MySqlDbContext _context = context;

    public async Task<ConversionTool> AddAsync(ConversionTool entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<ConversionTool>().AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(ConversionTool entity, CancellationToken cancellationToken = default)
    {
        _context.Set<ConversionTool>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ConversionTool entity, CancellationToken cancellationToken = default)
    {
        _context.Set<ConversionTool>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<ConversionTool> entities, CancellationToken cancellationToken = default)
    {
        _context.Set<ConversionTool>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
