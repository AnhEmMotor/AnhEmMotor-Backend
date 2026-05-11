using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.HR.CommissionPolicy;

public class CommissionPolicyDeleteRepository(ApplicationDBContext context) : ICommissionPolicyDeleteRepository
{
    public void Remove(Domain.Entities.HR.CommissionPolicy policy)
    {
        context.CommissionPolicies.Remove(policy);
    }
}
