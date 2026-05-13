using Domain.Entities;
using CommissionPolicyEntity = Domain.Entities.CommissionPolicy;

namespace Application.Interfaces.Repositories.HR.CommissionPolicy;

public interface ICommissionPolicyInsertRepository
{
    public void Add(CommissionPolicyEntity policy);

    public void AddAuditLog(CommissionPolicyAuditLog auditLog);
}
