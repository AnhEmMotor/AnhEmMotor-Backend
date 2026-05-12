using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;

public class GetCommissionPolicyAuditLogsQueryHandler(ICommissionPolicyReadRepository repository) : IRequestHandler<GetCommissionPolicyAuditLogsQuery, List<CommissionPolicyAuditLog>>
{
    public Task<List<CommissionPolicyAuditLog>> Handle(
        GetCommissionPolicyAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetAuditLogsAsync(request.PolicyId, cancellationToken);
    }
}
