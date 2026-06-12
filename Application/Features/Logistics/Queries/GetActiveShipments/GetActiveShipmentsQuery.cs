using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Queries.GetActiveShipments;

public class GetActiveShipmentsQuery : IRequest<List<ActiveShipmentDto>>
{
}

public class ActiveShipmentDto
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string CustomerAddress { get; set; }
    public string Carrier { get; set; }
    public int Status { get; set; }
    public decimal CodAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpectedAt { get; set; }
    public int DaysInTransit { get; set; }
    public bool IsStuck { get; set; }
}

public class GetActiveShipmentsQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository)
    : IRequestHandler<GetActiveShipmentsQuery, List<ActiveShipmentDto>>
{
    public async Task<List<ActiveShipmentDto>> Handle(GetActiveShipmentsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var shipments = (await parcelDeliveryOrderReadRepository.GetAllAsync(cancellationToken))
            .Where(x => x.Status == ParcelDeliveryStatus.Shipping)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        var result = shipments.Select(x => new ActiveShipmentDto
        {
            Id = x.Id,
            TrackingNumber = x.TrackingNumber ?? string.Empty,
            CustomerName = x.CustomerName ?? string.Empty,
            CustomerPhone = x.CustomerPhone ?? string.Empty,
            CustomerAddress = x.CustomerAddress ?? string.Empty,
            Carrier = x.Carrier ?? string.Empty,
            Status = (int)x.Status,
            CodAmount = x.CodAmount,
            ShippingCost = x.ShippingCost,
            CreatedAt = x.CreatedAt,
            ExpectedAt = x.ExpectedAt,
            DaysInTransit = (int)(now - x.CreatedAt).TotalDays,
            IsStuck = (now - x.CreatedAt).TotalHours > 48
        }).ToList();

        return result;
    }
}
