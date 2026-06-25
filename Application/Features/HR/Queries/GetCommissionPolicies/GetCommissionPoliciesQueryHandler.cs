using Application.Common.Models;
using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicies;

public class GetCommissionPoliciesQueryHandler(ICommissionPolicyReadRepository repository) : IRequestHandler<GetCommissionPoliciesQuery, Result<List<CommissionPolicy>>>
{
    public async Task<Result<List<CommissionPolicy>>> Handle(
        GetCommissionPoliciesQuery request,
        CancellationToken cancellationToken)
    {
        var policies = await repository.GetPoliciesAsync(cancellationToken).ConfigureAwait(false);
        return policies;
    }
}
