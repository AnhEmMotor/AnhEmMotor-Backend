using Application.ApiContracts.SalesContracts.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq;

namespace Infrastructure.Repositories.SalesContract;

public class SalesContractReadRepository(ApplicationDBContext context, ISievePaginator paginator) : ISalesContractReadRepository
{
    internal IQueryable<Domain.Entities.SalesContract> GetQueryable()
    {
        return context.SalesContracts.AsQueryable();
    }

    public Task<PagedResult<SalesContractResponse>> GetPagedAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable();
        return paginator.ApplyAsync<Domain.Entities.SalesContract, SalesContractResponse>(
            query,
            sieveModel,
            null,
            cancellationToken);
    }

    public Task<Domain.Entities.SalesContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetQueryable().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<Domain.Entities.SalesContract>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return GetQueryable().ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.SalesContract?> GetByOrderIdAsync(
        int? orderId,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable().FirstOrDefaultAsync(x => x.OutputId == orderId, cancellationToken);
    }

    public Task<bool> IsContractNumberExistsAsync(
        string contractNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable().Where(x => x.ContractNumber == contractNumber);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return query.AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return GetQueryable().CountAsync(cancellationToken);
    }

    public Task<int> CountByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return GetQueryable().CountAsync(x => string.Compare(x.Status, status) == 0, cancellationToken);
    }

    public Task<int> CountOverdueAsync(CancellationToken cancellationToken = default)
    {
        return GetQueryable()
            .CountAsync(
                x => string.Compare(x.Status, "Signed") == 0 &&
                    x.FinalPaymentDeadline.HasValue &&
                    x.FinalPaymentDeadline.Value < DateTimeOffset.UtcNow,
                cancellationToken);
    }
}
