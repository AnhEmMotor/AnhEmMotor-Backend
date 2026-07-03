using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using ReturnRequestEntity = Domain.Entities.ReturnRequest;

namespace Infrastructure.Repositories.ReturnRequest;

public class ReturnRequestReadRepository : IReturnRequestReadRepository
{
    private readonly ApplicationDBContext _context;
    private readonly ISieveProcessor _sieveProcessor;

    public ReturnRequestReadRepository(ApplicationDBContext context, ISieveProcessor sieveProcessor)
    {
        _context = context;
        _sieveProcessor = sieveProcessor;
    }

    public async Task<ReturnRequestEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ReturnRequests
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<ReturnRequestEntity>> GetPagedAsync(SieveModel sieveModel, CancellationToken cancellationToken = default)
    {
        var query = _context.ReturnRequests.Include(x => x.Items).AsNoTracking();

        var totalCount = await _sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken);

        var items = await _sieveProcessor.Apply(sieveModel, query, applySorting: true, applyPagination: true)
            .ToListAsync(cancellationToken);

        var page = sieveModel.Page ?? 1;
        var pageSize = sieveModel.PageSize ?? 10;

        return new PagedResult<ReturnRequestEntity>(items, totalCount, page, pageSize);
    }
}
