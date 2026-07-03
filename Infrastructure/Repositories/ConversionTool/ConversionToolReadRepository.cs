using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.ConversionTool;

public class ConversionToolReadRepository(ApplicationDBContext context) : IConversionToolReadRepository
{
    private readonly ApplicationDBContext _context = context;

    public Task<List<Domain.Entities.ConversionTool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.Set<Domain.Entities.ConversionTool>()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.ConversionTool?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Set<Domain.Entities.ConversionTool>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
