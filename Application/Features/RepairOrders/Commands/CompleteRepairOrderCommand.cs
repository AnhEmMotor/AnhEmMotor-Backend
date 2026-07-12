using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public class CompleteRepairOrderCommand : IRequest<Result<bool>>
{
    public int RepairOrderId { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
