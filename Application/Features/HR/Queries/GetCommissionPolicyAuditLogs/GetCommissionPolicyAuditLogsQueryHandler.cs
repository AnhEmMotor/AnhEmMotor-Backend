using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;

public class GetCommissionPolicyAuditLogsQueryHandler(ICommissionPolicyRepository repository) : IRequestHandler<GetCommissionPolicyAuditLogsQuery, List<CommissionPolicyAuditLog>>
{
    public Task<List<CommissionPolicyAuditLog>> Handle(
        GetCommissionPolicyAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetAuditLogsAsync(request.PolicyId, cancellationToken);
    }
}
