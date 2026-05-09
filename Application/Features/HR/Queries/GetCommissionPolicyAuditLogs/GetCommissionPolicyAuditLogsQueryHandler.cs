using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;

public class GetCommissionPolicyAuditLogsQueryHandler(ICommissionPolicyRepository repository)
    : IRequestHandler<GetCommissionPolicyAuditLogsQuery, List<CommissionPolicyAuditLog>>
{
    public Task<List<CommissionPolicyAuditLog>> Handle(
        GetCommissionPolicyAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetAuditLogsAsync(request.PolicyId, cancellationToken);
    }
}
