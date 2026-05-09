using Domain.Entities.HR;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;

public record GetCommissionPolicyAuditLogsQuery(int PolicyId) : IRequest<List<CommissionPolicyAuditLog>>;
