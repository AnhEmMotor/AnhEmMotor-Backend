using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierDebt;
using Domain.Constants;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.DebtPayments.Queries.GetDebtPaymentAuditLogs;

public class GetDebtPaymentAuditLogsQueryHandler(ISupplierDebtReadRepository repository)
    : IRequestHandler<GetDebtPaymentAuditLogsQuery, Result<List<DebtPaymentAuditLogResponse>>>
{
    public async Task<Result<List<DebtPaymentAuditLogResponse>>> Handle(
        GetDebtPaymentAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await repository.GetSupplierDebtAuditLogsAsync(request.Id, cancellationToken);
        
        var response = logs.Select(log => new DebtPaymentAuditLogResponse
        {
            Id = log.Id,
            Action = log.Action,
            ChangedByFullName = log.ChangedBy?.FullName,
            ChangedAt = log.ChangedAt,
            OldAmount = log.OldAmount,
            NewAmount = log.NewAmount,
            OldNotes = log.OldNotes,
            NewNotes = log.NewNotes,
            OldPaymentDate = log.OldPaymentDate,
            NewPaymentDate = log.NewPaymentDate
        }).ToList();

        return response;
    }
}
