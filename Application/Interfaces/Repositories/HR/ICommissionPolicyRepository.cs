using Domain.Entities.HR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.HR;

public interface ICommissionPolicyRepository
{
    public Task<List<CommissionPolicy>> GetPoliciesAsync(CancellationToken cancellationToken = default);
    public Task<List<CommissionPolicyAuditLog>> GetAuditLogsAsync(int policyId, CancellationToken cancellationToken = default);
    public Task<CommissionPolicy?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    public void Remove(CommissionPolicy policy);
}
