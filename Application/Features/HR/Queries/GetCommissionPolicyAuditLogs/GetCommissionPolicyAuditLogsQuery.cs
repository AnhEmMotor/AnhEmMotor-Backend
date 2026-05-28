using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;

public record GetCommissionPolicyAuditLogsQuery(int PolicyId) : IRequest<Result<List<CommissionPolicyAuditLog>>>;

