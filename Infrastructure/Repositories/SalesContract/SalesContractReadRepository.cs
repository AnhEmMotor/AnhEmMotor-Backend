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
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(contract =>
                contract.ContractNumber.Contains(normalizedKeyword) ||
                (contract.CustomerFullName != null && contract.CustomerFullName.Contains(normalizedKeyword)) ||
                (contract.CustomerCCCD != null && contract.CustomerCCCD.Contains(normalizedKeyword)) ||
                (contract.CustomerPhone != null && contract.CustomerPhone.Contains(normalizedKeyword)) ||
                (contract.FrameNumber != null && contract.FrameNumber.Contains(normalizedKeyword)) ||
                (contract.EngineNumber != null && contract.EngineNumber.Contains(normalizedKeyword)));
        }
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

    public Task<List<Domain.Entities.SalesContract>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable()
            .Where(x => x.CustomerId == customerId && x.DeletedAt == null)
            .OrderByDescending(x => x.SignedDate)
            .ToListAsync(cancellationToken);
    }
}
