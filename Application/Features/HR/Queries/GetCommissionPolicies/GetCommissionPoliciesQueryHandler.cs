using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public class GetCommissionPoliciesQueryHandler(ICommissionPolicyRepository repository)
    : IRequestHandler<GetCommissionPoliciesQuery, List<CommissionPolicy>>
{
    public Task<List<CommissionPolicy>> Handle(
        GetCommissionPoliciesQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetPoliciesAsync(cancellationToken);
    }
}
