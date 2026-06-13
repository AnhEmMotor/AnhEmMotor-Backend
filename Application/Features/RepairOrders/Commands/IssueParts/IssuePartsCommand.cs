using Application.ApiContracts.RepairOrder.Requests;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.RepairOrders.Commands.IssueParts
{
    public class IssuePartsCommand : IRequest<Result<bool>>
    {
        public int RepairOrderId { get; set; }
        public List<PartItemRequest> Parts { get; set; } = [];
        public List<ServiceItemRequest> Services { get; set; } = [];
        public string? Status { get; set; } // Optional: transition to "QcPending" or keep "InProgress"
    }
}
