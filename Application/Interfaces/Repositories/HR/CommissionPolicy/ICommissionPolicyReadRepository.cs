using Domain.Entities;
using CommissionPolicyEntity = Domain.Entities.CommissionPolicy;

namespace Application.Interfaces.Repositories.HR.CommissionPolicy;

public interface ICommissionPolicyReadRepository
{
    public Task<List<CommissionPolicyEntity>> GetPoliciesAsync(CancellationToken cancellationToken = default);

    public Task<List<CommissionPolicyAuditLog>> GetAuditLogsAsync(
        int policyId,
        CancellationToken cancellationToken = default);

    public Task<CommissionPolicyEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<CommissionPolicyEntity?> GetExistingPolicyAsync(
        int? productId,
        int? categoryId,
        DateTimeOffset effectiveDate,
        CancellationToken cancellationToken = default);
}

