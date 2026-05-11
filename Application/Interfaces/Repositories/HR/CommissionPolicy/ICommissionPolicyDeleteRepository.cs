using CommissionPolicyEntity = Domain.Entities.HR.CommissionPolicy;

namespace Application.Interfaces.Repositories.HR.CommissionPolicy;

public interface ICommissionPolicyDeleteRepository
{
    public void Remove(CommissionPolicyEntity policy);
}
