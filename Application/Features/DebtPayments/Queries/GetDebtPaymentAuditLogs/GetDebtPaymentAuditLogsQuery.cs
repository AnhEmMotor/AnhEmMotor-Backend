using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.DebtPayments.Queries.GetDebtPaymentAuditLogs;

public class GetDebtPaymentAuditLogsQuery : IRequest<Result<List<DebtPaymentAuditLogResponse>>>
{
    public int Id { get; set; }
}
