using Application.Interfaces.Repositories.ReturnRequest;
using Domain.Entities;
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

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return _context.ReturnRequests.CountAsync(cancellationToken);
    }

    public async Task<PagedResult<ReturnRequestEntity>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ReturnRequests
            .Include(x => x.Items)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);
        var totalCount = await _sieveProcessor.Apply(sieveModel, query, applyPagination: false)
            .CountAsync(cancellationToken);
        var items = await _sieveProcessor.Apply(sieveModel, query, applySorting: true, applyPagination: true)
            .ToListAsync(cancellationToken);
        var page = sieveModel.Page ?? 1;
        var pageSize = sieveModel.PageSize ?? 10;
        return new PagedResult<ReturnRequestEntity>(items, totalCount, page, pageSize);
    }

    public Task<bool> HasActiveReturnRequestAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return _context.ReturnRequests.AnyAsync(
            x => x.OrderId == orderId && x.Status != "rejected",
            cancellationToken);
    }

    public async Task<List<ReturnRequestEntity>> GetCompletedRestockAwaitingArrivalAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReturnRequests
            .Include(x => x.Items)
            .AsNoTracking()
            .Where(
                x => x.OrderId == orderId &&
                    x.Status == "completed" &&
                    x.ReturnAction == "restock" &&
                    !_context.InventoryReceipts.Any(
                        i => i.SourceOrderId == x.OrderId &&
                            i.Notes == $"Restock from Return Request #{x.Id}"))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ReturnRequestEntity>> GetByOrderIdAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReturnRequests
            .Include(x => x.Items)
            .Where(x => x.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }
}
