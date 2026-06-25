using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReceipts.Queries.GetInventoryReceiptAuditLogs;

public class GetInventoryReceiptAuditLogsQuery : IRequest<Result<List<InventoryReceiptAuditLogResponse>>>
{
    public int Id { get; set; }
}
