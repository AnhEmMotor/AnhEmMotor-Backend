using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public class IssuePartsCommand : IRequest<Result<bool>>
{
    public int RepairOrderId { get; set; }

    public List<PartItemDto> Parts { get; set; } = [];

    public List<ServiceItemDto> Services { get; set; } = [];
}

public class PartItemDto
{
    public int ProductVariantId { get; set; }

    public int Count { get; set; }

    public decimal Price { get; set; }

    public string? Notes { get; set; }
}

public class ServiceItemDto
{
    public int ServiceId { get; set; }

    public decimal LaborCost { get; set; }

    public string? Notes { get; set; }
}
