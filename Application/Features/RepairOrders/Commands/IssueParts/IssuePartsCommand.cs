using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.RepairOrders.Commands.IssueParts
{
    public class PartItem
    {
        public int ProductVariantId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
    }

    public class ServiceItem
    {
        public int ServiceId { get; set; }
        public decimal LaborCost { get; set; }
        public string? Notes { get; set; }
    }

    public class IssuePartsCommand : IRequest<Result<bool>>
    {
        public int RepairOrderId { get; set; }
        public List<PartItem> Parts { get; set; } = new List<PartItem>();
        public List<ServiceItem> Services { get; set; } = new List<ServiceItem>();
        public string? Status { get; set; } // Optional: transition to "QcPending" or keep "InProgress"
    }
}
