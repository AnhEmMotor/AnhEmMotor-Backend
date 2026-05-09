using Domain.Entities.HR;

namespace Application.Interfaces.Repositories.HR;

public interface ICommissionPolicyRepository
{
    public Task<List<CommissionPolicy>> GetPoliciesAsync(CancellationToken cancellationToken = default);

    public Task<List<CommissionPolicyAuditLog>> GetAuditLogsAsync(
        int policyId,
        CancellationToken cancellationToken = default);

    public Task<CommissionPolicy?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public void Remove(CommissionPolicy policy);
}
