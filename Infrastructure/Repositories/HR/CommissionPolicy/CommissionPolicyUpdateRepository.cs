using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR.CommissionPolicy;

public class CommissionPolicyUpdateRepository(ApplicationDBContext context) : ICommissionPolicyUpdateRepository
{
    public void Update(Domain.Entities.CommissionPolicy policy)
    {
        context.CommissionPolicies.Update(policy);
    }

    public void Restore(Domain.Entities.CommissionPolicy policy)
    {
        policy.IsActive = true;
        context.CommissionPolicies.Update(policy);
    }
}
