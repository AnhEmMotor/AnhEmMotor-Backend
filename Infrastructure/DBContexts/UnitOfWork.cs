using Application.Interfaces.Repositories;

namespace Infrastructure.DBContexts;

public class UnitOfWork(ApplicationDBContext context) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}