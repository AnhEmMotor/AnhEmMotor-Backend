using Application.Common.Models;
using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;

public class GetCommissionPolicyAuditLogsQueryHandler(ICommissionPolicyReadRepository repository) : IRequestHandler<GetCommissionPolicyAuditLogsQuery, Result<List<CommissionPolicyAuditLog>>>
{
    public async Task<Result<List<CommissionPolicyAuditLog>>> Handle(
        GetCommissionPolicyAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        return await repository.GetAuditLogsAsync(request.PolicyId, cancellationToken).ConfigureAwait(false);
    }
}

