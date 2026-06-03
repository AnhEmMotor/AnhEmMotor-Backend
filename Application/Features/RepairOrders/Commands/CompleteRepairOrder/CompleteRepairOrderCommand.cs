using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Commands.CompleteRepairOrder
{
    public class CompleteRepairOrderCommand : IRequest<Result<bool>>
    {
        public int RepairOrderId { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string PaymentStatus { get; set; } = "Paid";
        public string? Notes { get; set; }
    }
}
