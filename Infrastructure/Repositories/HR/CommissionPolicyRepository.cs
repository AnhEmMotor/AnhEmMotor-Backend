using Application.Interfaces;
using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.HR;

public class CommissionPolicyRepository(IApplicationDBContext context) : ICommissionPolicyRepository
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

    public void Remove(CommissionPolicy policy)
    {
        context.CommissionPolicies.Remove(policy);
    }
}
