using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public class GetCommissionPoliciesQueryHandler(ICommissionPolicyRepository repository) : IRequestHandler<GetCommissionPoliciesQuery, List<CommissionPolicy>>
{
    public Task<List<CommissionPolicy>> Handle(GetCommissionPoliciesQuery request, CancellationToken cancellationToken)
    {
        return repository.GetPoliciesAsync(cancellationToken);
    }
}
