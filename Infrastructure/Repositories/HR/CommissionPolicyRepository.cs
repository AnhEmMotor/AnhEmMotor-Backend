using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.HR;

public class CommissionPolicyRepository(ApplicationDBContext context) : ICommissionPolicyRepository
{
    public Task<List<CommissionPolicy>> GetPoliciesAsync(CancellationToken cancellationToken = default)
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

    public Task<CommissionPolicy?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.CommissionPolicies.FindAsync([id], cancellationToken).AsTask();
    }

    public Task<CommissionPolicy?> GetExistingPolicyAsync(
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

    public void Add(CommissionPolicy policy)
    {
        context.CommissionPolicies.Add(policy);
    }

    public void AddAuditLog(CommissionPolicyAuditLog auditLog)
    {
        context.CommissionPolicyAuditLogs.Add(auditLog);
    }

    public void Remove(CommissionPolicy policy)
    {
        context.CommissionPolicies.Remove(policy);
    }
}
