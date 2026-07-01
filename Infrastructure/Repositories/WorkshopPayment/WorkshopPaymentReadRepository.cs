using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WorkshopPayment;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.WorkshopPayment;

public class WorkshopPaymentReadRepository : IWorkshopPaymentReadRepository
{
    private readonly ApplicationDBContext _context;
    private readonly ISieveProcessor _sieveProcessor;
    private readonly ISievePaginator _paginator;

    public WorkshopPaymentReadRepository(ApplicationDBContext context, ISieveProcessor sieveProcessor, ISievePaginator paginator)
    {
        _context = context;
        _sieveProcessor = sieveProcessor;
        _paginator = paginator;
    }

    public async Task<PagedResult<Domain.Entities.WorkshopPayment>> GetPagedAsync(SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = _context.WorkshopPayments.AsNoTracking().Where(x => x.DeletedAt == null);
        var paged = await _paginator.ApplyAsync<Domain.Entities.WorkshopPayment, Domain.Entities.WorkshopPayment>(
            query, sieveModel, cancellationToken: cancellationToken);
        return paged;
    }

    public async Task<Domain.Entities.WorkshopPayment?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.WorkshopPayments.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
    }

    public async Task<object> GetStatsAsync(CancellationToken cancellationToken)
    {
        var today = System.DateTimeOffset.UtcNow.Date;
        var startOfMonth = new System.DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, System.TimeSpan.Zero);
        
        var query = _context.WorkshopPayments.AsNoTracking().Where(x => x.DeletedAt == null);

        var unpaid = await query.Where(x => x.PaymentStatus == "Unpaid").CountAsync(cancellationToken);
        var unpaidAmount = await query.Where(x => x.PaymentStatus == "Unpaid").SumAsync(x => (double?)x.TotalAmount ?? 0, cancellationToken);

        var partial = await query.Where(x => x.PaymentStatus == "Partial").CountAsync(cancellationToken);
        var partialAmount = await query.Where(x => x.PaymentStatus == "Partial").SumAsync(x => (double?)x.TotalAmount ?? 0, cancellationToken);

        var paidTodayQuery = query.Where(x => x.PaymentStatus == "Paid" && x.PaidAt != null && x.PaidAt >= today);
        var paidToday = await paidTodayQuery.CountAsync(cancellationToken);
        var paidTodayAmount = await paidTodayQuery.SumAsync(x => (double?)x.TotalAmount ?? 0, cancellationToken);

        var monthRevenue = await query.Where(x => x.PaymentStatus == "Paid" && x.PaidAt != null && x.PaidAt >= startOfMonth)
                                      .SumAsync(x => (double?)x.TotalAmount ?? 0, cancellationToken);

        return new
        {
            unpaid,
            unpaidAmount,
            partial,
            partialAmount,
            paidToday,
            paidTodayAmount,
            monthRevenue
        };
    }
}
