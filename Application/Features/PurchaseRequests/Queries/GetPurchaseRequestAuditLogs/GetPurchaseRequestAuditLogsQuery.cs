using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequestAuditLogs
{
    public class GetPurchaseRequestAuditLogsQuery : IRequest<Result<List<PurchaseRequestAuditLogResponse>>>
    {
        public int Id { get; set; }
    }
}
