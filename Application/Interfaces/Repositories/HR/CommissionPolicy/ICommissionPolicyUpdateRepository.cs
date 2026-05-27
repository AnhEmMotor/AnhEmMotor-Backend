using CommissionPolicyEntity = Domain.Entities.CommissionPolicy;

namespace Application.Interfaces.Repositories.HR.CommissionPolicy;

public interface ICommissionPolicyUpdateRepository
{
    public void Update(CommissionPolicyEntity policy);

    public void Restore(CommissionPolicyEntity policy);
}
