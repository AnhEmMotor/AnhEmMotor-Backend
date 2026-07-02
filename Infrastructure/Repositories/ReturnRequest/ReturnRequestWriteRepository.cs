using Application.Interfaces.Repositories.ReturnRequest;
using Infrastructure.DBContexts;
using ReturnRequestEntity = Domain.Entities.ReturnRequest;

namespace Infrastructure.Repositories.ReturnRequest;

public class ReturnRequestWriteRepository : IReturnRequestWriteRepository
{
    private readonly ApplicationDBContext _context;

    public ReturnRequestWriteRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ReturnRequestEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.ReturnRequests.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(ReturnRequestEntity entity, CancellationToken cancellationToken = default)
    {
        _context.ReturnRequests.Update(entity);
        return Task.CompletedTask;
    }
}
