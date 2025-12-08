using Application.Interfaces.Repositories;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories;

public class UnitOfWork(ApplicationDBContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    { await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false); }
}