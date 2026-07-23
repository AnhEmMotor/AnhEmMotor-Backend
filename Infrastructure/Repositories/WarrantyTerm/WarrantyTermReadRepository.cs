using Application.Interfaces.Repositories.WarrantyTerm;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.WarrantyTerm;

public class WarrantyTermReadRepository(ApplicationDBContext context) : IWarrantyTermReadRepository
{
    public async Task<List<Domain.Entities.WarrantyTerm>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Set<Domain.Entities.WarrantyTerm>()
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Domain.Entities.WarrantyTerm?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Set<Domain.Entities.WarrantyTerm>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }
}
