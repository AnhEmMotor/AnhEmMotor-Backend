using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR.CommissionPolicy;

public class CommissionPolicyInsertRepository(ApplicationDBContext context) : ICommissionPolicyInsertRepository
{
    public void Add(Domain.Entities.CommissionPolicy policy)
    {
        context.CommissionPolicies.Add(policy);
    }

    public void AddAuditLog(CommissionPolicyAuditLog auditLog)
    {
        context.CommissionPolicyAuditLogs.Add(auditLog);
    }
}
