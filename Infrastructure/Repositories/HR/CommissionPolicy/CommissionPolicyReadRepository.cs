using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.HR.CommissionPolicy;

public class CommissionPolicyReadRepository(ApplicationDBContext context) : ICommissionPolicyReadRepository
{
    public Task<List<Domain.Entities.CommissionPolicy>> GetPoliciesAsync(CancellationToken cancellationToken = default)
    {
        return context.CommissionPolicies
            .Include(p => p.Category)
            .Include(p => p.Product)
            .OrderByDescending(p => p.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<CommissionPolicyAuditLog>> GetAuditLogsAsync(
        int policyId,
        CancellationToken cancellationToken = default)
    {
        return context.CommissionPolicyAuditLogs
            .Where(l => l.PolicyId == policyId)
            .OrderByDescending(l => l.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.CommissionPolicy?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.CommissionPolicies.FindAsync([id], cancellationToken).AsTask();
    }

    public Task<Domain.Entities.CommissionPolicy?> GetExistingPolicyAsync(
        int? productId,
        int? categoryId,
        DateTimeOffset effectiveDate,
        CancellationToken cancellationToken = default)
    {
        var startDate = effectiveDate.AddDays(-7);
        var endDate = effectiveDate.AddDays(7);
        return context.CommissionPolicies
            .Where(
                p => p.IsActive &&
                    p.ProductId == productId &&
                    p.CategoryId == categoryId &&
                    p.EffectiveDate > startDate &&
                    p.EffectiveDate < endDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
