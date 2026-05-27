using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public class GetCommissionPoliciesQueryHandler(ICommissionPolicyReadRepository repository) : IRequestHandler<GetCommissionPoliciesQuery, List<CommissionPolicy>>
{
    public Task<List<CommissionPolicy>> Handle(GetCommissionPoliciesQuery request, CancellationToken cancellationToken)
    {
        return repository.GetPoliciesAsync(cancellationToken);
    }
}
