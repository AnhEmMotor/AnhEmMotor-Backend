using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Returns.Queries.GetReturns;

public class GetReturnsQuery : IRequest<List<ReturnOrderDto>>
{
    public string? Status { get; set; } // "pending", "inspecting", "completed", or null for all
}

public class ReturnOrderDto
{
    public int Id { get; set; }
    public string OriginalTrackingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GetReturnsQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository)
    : IRequestHandler<GetReturnsQuery, List<ReturnOrderDto>>
{
    public async Task<List<ReturnOrderDto>> Handle(GetReturnsQuery request, CancellationToken cancellationToken)
    {
        var returns = await parcelDeliveryOrderReadRepository.GetReturnedAsync(cancellationToken);
        var query = returns.AsQueryable();
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (request.Status == "pending") query = query.Where(x => x.InspectedAt == null);
            else if (request.Status == "completed") query = query.Where(x => x.ReturnAction != null);
            else if (request.Status == "inspecting") query = query.Where(x => x.InspectedAt != null && x.ReturnAction == null);
        }
        returns = query.OrderByDescending(x => x.CreatedAt).ToList();

        var result = returns.Select(x => new ReturnOrderDto
        {
            Id = x.Id,
            OriginalTrackingNumber = x.TrackingNumber ?? string.Empty,
            CustomerName = x.CustomerName ?? string.Empty,
            Carrier = x.Carrier ?? string.Empty,
            Reason = x.ReturnReason ?? "Khách bom hàng",
            Status = GetReturnStatus(x),
            CreatedAt = x.CreatedAt
        }).ToList();

        return result;
    }

    private string GetReturnStatus(ParcelDeliveryOrder order)
    {
        if (order.ReturnAction != null) return "completed";
        if (order.InspectedAt != null) return "inspecting";
        return "pending";
    }
}
