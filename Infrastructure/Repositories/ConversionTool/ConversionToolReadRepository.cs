using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.ConversionTool;

public class ConversionToolReadRepository(MySqlDbContext context) : IConversionToolReadRepository
{
    private readonly MySqlDbContext _context = context;

    public Task<List<ConversionTool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.Set<ConversionTool>()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ConversionTool?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Set<ConversionTool>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
