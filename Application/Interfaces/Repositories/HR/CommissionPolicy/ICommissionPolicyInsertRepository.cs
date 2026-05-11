using Domain.Entities;
using Domain.Entities.HR;
using CommissionPolicyEntity = Domain.Entities.HR.CommissionPolicy;

namespace Application.Interfaces.Repositories.HR.CommissionPolicy;

public interface ICommissionPolicyInsertRepository
{
    public void Add(CommissionPolicyEntity policy);

    public void AddAuditLog(CommissionPolicyAuditLog auditLog);
}
