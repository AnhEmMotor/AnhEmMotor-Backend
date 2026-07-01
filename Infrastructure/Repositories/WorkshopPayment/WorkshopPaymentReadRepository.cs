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
}
